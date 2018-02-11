#!/usr/bin/env python3
# -*- coding: utf-8 -*-



import os
import sys




from ._CommunicationPipe import _CommunicationPipe







#
# This class establishes a communication bridge to another process. In that case the current process will manage the components attached to this process.
# Consider this class as some kind of low level implementation: The communication protocol between this process using this class and other processes attached
# is kept very simple. In order to work with functionality provided in other processes you need to know exactly their name and calling conventions.
#
class SimpleInterProcessCommunicationBridge(object):



	def __init__(self):
		self.__pipesList = tuple()
		self.__pipesMap = {}
	#



	#
	# Launch a component process. From now on STDIN and STDOUT will be used to communicate with this process.
	#
	# @param	str name				The name of the new process. You can later communicate with this process using this name.
	# @param	str execFilePath		The (absolute) path to the executable.
	# @param	str outgoingDataType	An identifier that describes the type of data to serialize:
	#									* <c>bin</c> : The data to send must be binary data later.
	#									* <c>json</c> : The data to send must be a JSON object later.
	#									* <c>str</c> : The data to send must be a string later.
	#
	def launchComponentProcess(self, name, execFilePath, outgoingDataType, incomingDataType):
		assert isinstance(name, str)
		assert isinstance(execFilePath, str)
		assert os.path.isfile(execFilePath)
		assert isinstance(outgoingDataType, str)
		assert outgoingDataType in ["bin", "json", "str"]
		assert isinstance(incomingDataType, str)
		assert incomingDataType in ["bin", "json", "str"]

		if name in self.__pipesMap:
			raise Exception("A process with name '" + name + "' already exists!")
		p = _CommunicationPipe(self, name, execFilePath, outgoingDataType, incomingDataType)
		self.__pipesList = self.__pipesList + (p,)
		self.__pipesMap[name] = p
	#



	@property
	def names(self):
		return list(self.__pipesMap.keys())
	#


	#
	# Synchroneously sends data to an attached process.
	#
	# @param	str name				The name of the component you want to send data to.
	# @param	str targetID			The software handler within the component to address.
	# @param	mixed data				The data to send.
	# @return	tuple<bool,mixed>		A status tuple:
	#									* bool bSuccess : Gives information about success or error of the attempt to send data.
	#									* mixed retData : The return data. On error this might be an exception object with details about the problem encountered. If no such object is available assume the process simply has been killed or died.
	#
	def send(self, name, targetID, data):
		assert isinstance(name, str)
		assert isinstance(targetID, str)

		# print(" -- " + name)
		# print(" -- " + str(self.__pipesMap))
		p = self.__pipesMap.get(name, None)
		if p is None:
			raise Exception("No such component: '" + name + "'")
		nResultInfo, retData = p.send(targetID, data)
		# print(nResultInfo, retData)
		if nResultInfo < 0:
			p.kill()
			del self.__pipesMap[name]
			return (False, retData)
		else:
			return (True, retData)
	#



	#
	# Kill a remote process forcefully.
	#
	# @param	str name				The name of the component you want to kill.
	#
	def kill(self, name):
		assert isinstance(name, str)

		p = self.__pipesMap.get(name, None)
		if p is None:
			raise Exception("No such component: '" + name + "'")
		p.kill()
		del self.__pipesMap[name]
	#



#





