#!/usr/bin/env python3
# -*- coding: utf-8 -*-



import sys
import struct
import ctypes




class WriteBuffer(object):

	def __init__(self):
		self.__buffer = bytearray()
	#

	def writeInt32(self, value):
		assert isinstance(value, int)
		self.__buffer.extend(struct.pack("<i", value))
	#

	def writeInt64(self, value):
		assert isinstance(value, int)
		self.__buffer.extend(struct.pack("<q", value))
	#

	def writeStringU8(self, value):
		assert isinstance(value, str)
		binaryData = value.encode("utf-8")
		rawLength = len(binaryData)
		if rawLength > 254:
			raise Exception("String exceeds 254 values in UTF-8 binary data!")
		self.__buffer.append(rawLength)
		self.__buffer.extend(binaryData)
	#

	def writeStringU32(self, value):
		assert isinstance(value, str)
		binaryData = value.encode("utf-8")
		self.__buffer.extend(struct.pack("<i", len(binaryData)))
		self.__buffer.extend(binaryData)
	#

	def writeArray32(self, value):
		assert isinstance(value, (bytes, bytearray))
		self.__buffer.extend(struct.pack("<i", len(value)))
		self.__buffer.extend(value)
	#

	def toArray(self):
		return self.__buffer
	#

#






