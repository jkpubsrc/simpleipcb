#!/usr/bin/env python3
# -*- coding: utf-8 -*-



import threading
import os
import json
import subprocess
import struct
import select
import fcntl




from .ReadBuffer import ReadBuffer
from .WriteBuffer import WriteBuffer










class _CommunicationPipe(object):

	def __init__(self, parent, name, execFilePath, outgoingDataType, incomingDataType):
		self.__parent = parent
		self.__name = name
		self.__execFilePath = execFilePath
		self.__p = subprocess.Popen(execFilePath, shell=False, stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE, universal_newlines=False)
		self.__evt = threading.Event()
		self.__outgoingMessage = None
		self.__incomingMessage = None
		self.__outgoingDataType = outgoingDataType
		self.__incomingDataType = incomingDataType
		self.__readBuffer = ReadBuffer()

		fd = self.__p.stdout.fileno()
		fl = fcntl.fcntl(fd, fcntl.F_GETFL)
		fcntl.fcntl(fd, fcntl.F_SETFL, fl | os.O_NONBLOCK)

		self.__streams = [ self.__p.stdout ]
		self.__temp0 = []
		self.__temp = bytearray(65536)
	#



	def name(self):
		return self.__name
	#



	def kill(self):
		try:
			self.__p.kill()
			return True
		except Exception as e:
			return False
	#



	#
	# Send some data.
	#
	# @return		tuple<int,mixed>		Returns a tuple containing the following data:
	#										* int nResultInfo
	#											* 1: success
	#											* -1: failed to send data; the process will likely have died
	#											* -2: failed to receive data; the process will likely have died
	#											* -3: failed to decode the data received; process should be killed
	#										* mixed result : On success the data is returned as created by the component process. On error an exception object is returned or <c>None</c> if no exception occurred.
	#
	def send(self, targetID, data):
		binPacketData = self.__packPacket(targetID, data)
		#for v in binPacketData:
		#	print("0x{:02x}, ".format(v), end="")
		#print()

		try:
			# print("-- sending packet of size " + str(len(binPacketData)))
			self.__p.stdin.write(binPacketData)
			self.__p.stdin.flush()
		except Exception as e:
			# print("-- " + str(e))
			return (-1, e)

		while True:
			if self.__p.poll() != None:
				# print("-- process has terminated")
				return (-2, None)

			#print("-- Waiting for data ...")
			readable, writable, exceptional = select.select(self.__streams, self.__temp0, self.__temp0, 2)
			if len(readable) == 0:
				# print("-- No data.")
				continue

			# print("-- Reading into ...")
			numberOfBytesReceived = self.__p.stdout.readinto1(self.__temp)
			# print("-- number of bytes received: " + str(numberOfBytesReceived))
			if numberOfBytesReceived <= 0:
				if self.__p.poll() != None:
					# print("-- process has terminated")
					return (-2, None)
				# print("-- no data received")
				return (-2, Exception("No data received!"))

			self.__readBuffer.appendData(self.__temp, numberOfBytesReceived)
			# print("-- Trying to parse ...")
			nSuccess, pkg = self.__tryReadAndParsePacket(self.__readBuffer)

			if nSuccess == 0:
				# not enough data
				# print("-- not enough data")
				# print("---- waiting in buffer: " + str(self.__readBuffer.bytesAvailable))
				continue
			elif nSuccess == -1:
				# failed to parse data
				# print("-- failed to parse data")
				return (-3, pkg)
			else:
				# print("-- Success!")
				# print("-- success")
				return 1, pkg
	#



	def __packPacket(self, targetID, data):
		if self.__outgoingDataType == "bin":
			if isinstance(data, (bytes, bytearray)):
				rawArgumentData = data
		elif self.__outgoingDataType == "json":
			if isinstance(data, (str, int, bool, str, list, tuple, dict)):
				rawArgumentData = json.dumps(data).encode("utf-8")
		elif self.__outgoingDataType == "str":
			if isinstance(data, str):
				rawArgumentData = data.encode("utf-8")
		else:
			raise Exception("Unknown data type keyword: " + self.__outgoingDataType)

		buffer = WriteBuffer()
		buffer.writeInt32(0)
		buffer.writeStringU8(targetID)
		buffer.writeArray32(rawArgumentData)

		data = buffer.toArray()
		data[0:4] = struct.pack("<i", len(data) - 4)

		return data
	#



	#
	# Invoked by another thread: Try to receive data.
	#
	def __tryReadAndParsePacket(self, inputBuffer):
		assert isinstance(inputBuffer, ReadBuffer)

		available = inputBuffer.bytesAvailable
		# print(">> Number of bytes available: " + str(available))
		pos = inputBuffer.position
		rawData, newPos = inputBuffer.peekArray32(pos)
		# print((rawData, newPos))
		if newPos is None:
			# print(">> Failed to read array32")
			return (0, None)
		inputBuffer.position = newPos
		# print(">> Discarding old data")
		inputBuffer.discardOldData()
		# print(rawData)

		# print(">> Decoding ...")
		if self.__incomingDataType == "bin":
			return (1, rawData)
		elif self.__incomingDataType == "str":
			try:
				return (1, rawData.decode("utf-8"))
			except Exception as e:
				return (-1, e)
		elif self.__incomingDataType == "json":
			try:
				return (1, json.loads(rawData.decode("utf-8")))
			except Exception as e:
				return (-1, e)
		else:
			return (-1, Exception("Unknown data type keyword: " + self.__incomingDataType))
	#

#






