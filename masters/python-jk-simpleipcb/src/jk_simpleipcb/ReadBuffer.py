#!/usr/bin/env python3
# -*- coding: utf-8 -*-



import sys
import struct
import ctypes




class ReadBuffer(object):

	def __init__(self):
		self.__buffer = bytearray()
		self.__pos = 0
		self.__capacity = 0
	#

	def __len__(self):
		return self.__capacity
	#

	@property
	def position(self):
		return self.__pos
	#

	@position.setter
	def position(self, value):
		if value < 0:
			value = 0
		if value > len(self.__buffer):
			value = len(self.__buffer)
		self.__pos = value
	#

	@property
	def bytesAvailable(self):
		return self.__capacity - self.__pos
	#

	def readInt32(self):
		bytesAvailable = self.__capacity - self.__pos
		if bytesAvailable < 4:
			raise Exception("Not enough data available: 4 bytes expected, " + str(bytesAvailable) + " bytes available!")
		v = struct.unpack_from("<i", self.__buffer, self.__pos)[0]
		self.__pos += 4
		return v
	#

	def peekInt32(self, pos):
		bytesAvailable = self.__capacity - pos
		if bytesAvailable < 4:
			return (None, None)
		v = struct.unpack_from("<i", self.__buffer, pos)[0]
		pos += 4
		return (v, pos)
	#

	def readInt64(self):
		bytesAvailable = self.__capacity - self.__pos
		if bytesAvailable < 8:
			raise Exception("Not enough data available: 4 bytes expected, " + str(bytesAvailable) + " bytes available!")
		v = struct.unpack_from("<q", self.__buffer, self.__pos)[0]
		self.__pos += 8
		return v
	#

	def peekInt64(self, pos):
		bytesAvailable = self.__capacity - pos
		if bytesAvailable < 8:
			return (None, None)
		v = struct.unpack_from("<q", self.__buffer, pos)[0]
		pos += 8
		return (v, pos)
	#

	def readStringU8(self):
		bytesAvailable = self.__capacity - self.__pos
		if bytesAvailable < 1:
			raise Exception("Not enough data available: 1 byte expected, " + str(bytesAvailable) + " bytes available!")
		v = self.__buffer[self.__pos]
		self.__pos += 1
		if v < 0:
			raise Exception("Data error!")
		if v > bytesAvailable - 1:
			raise Exception("Not enough data available!")
		stringRawData = self.__buffer[self.__pos:self.__pos + v]
		s = stringRawData.decode("utf-8")
		self.__pos += v
		return s
	#

	def peekStringU8(self, pos):
		bytesAvailable = self.__capacity - pos
		if bytesAvailable < 1:
			return (None, None)
		v = self.__buffer[pos]
		pos += 1
		if v < 0:
			raise Exception("Data error!")
		if v > bytesAvailable - 1:
			return (None, None)
		stringRawData = self.__buffer[pos:pos + v]
		s = stringRawData.decode("utf-8")
		pos += v
		return s
	#

	def readStringU32(self):
		bytesAvailable = self.__capacity - self.__pos
		if bytesAvailable < 4:
			raise Exception("Not enough data available: 4 bytes expected, " + str(bytesAvailable) + " bytes available!")
		v = struct.unpack_from("<i", self.__buffer, self.__pos)[0]
		self.__pos += 4
		if v < 0:
			raise Exception("Data error!")
		if v > bytesAvailable - 4:
			raise Exception("Not enough data available!")
		stringRawData = self.__buffer[self.__pos:self.__pos + v]
		s = stringRawData.decode("utf-8")
		self.__pos += v
		return s
	#

	def peekStringU32(self, pos):
		bytesAvailable = self.__capacity - pos
		if bytesAvailable < 4:
			return (None, None)
		v = struct.unpack_from("<i", self.__buffer, pos)[0]
		pos += 4
		if v < 0:
			raise Exception("Data error!")
		if v > bytesAvailable - 4:
			return (None, None)
		stringRawData = self.__buffer[pos:pos + v]
		s = stringRawData.decode("utf-8")
		pos += v
		return (s, pos)
	#

	def readArray32(self):
		bytesAvailable = self.__capacity - self.__pos
		if bytesAvailable < 4:
			raise Exception("Not enough data available: 4 bytes expected, " + str(bytesAvailable) + " bytes available!")
		v = struct.unpack_from("<i", self.__buffer, self.__pos)[0]
		self.__pos += 4
		if v < 0:
			raise Exception("Data error!")
		if v > bytesAvailable - 4:
			raise Exception("Not enough data available!")
		rawData = self.__buffer[self.__pos:self.__pos + v]
		self.__pos += v
		return rawData
	#

	def peekArray32(self, pos):
		bytesAvailable = self.__capacity - pos
		if bytesAvailable < 4:
			return (None, None)
		v = struct.unpack_from("<i", self.__buffer, pos)[0]
		pos += 4
		if v < 0:
			raise Exception("Data error!")
		if v > bytesAvailable - 4:
			return (None, None)
		rawData = self.__buffer[pos:pos + v]
		pos += v
		return (rawData, pos)
	#

	def discardOldData(self):
		if self.__pos == 0:
			return
		n = self.__pos
		self.__buffer = bytearray(self.__buffer[self.__pos:])
		self.__pos = 0
		self.__capacity -= n
	#

	"""
	NOTE: as readinto() does not accept an offset the original strategy did not work. instead copying data multiple times can't be avoided.

	def getWriteArray(self, ensureCapacity):
		bytesAvailable = len(self.__buffer) - self.__capacity
		if bytesAvailable < ensureCapacity:
			n = 64
			while bytesAvailable + n < ensureCapacity:
				n *= 2
		self.__buffer.extend(bytes(n))
		return self.__buffer[self.__capacity:]
	#

	def notifyDataHasBeenWritten(self, numberOfBytesWritten):
		self.__capacity += numberOfBytesWritten
		print(self.__buffer[0:self.__capacity])
	#
	"""

	def appendData(self, data, length):
		self.__buffer.extend(data[0:length])
		self.__capacity += length
	#

	#def append(self, data):
	#	if isinstance(data, int):
	#		self.__buffer.append(data)
	#	else:
	#		self.__buffer.extend(data)
	#

#












