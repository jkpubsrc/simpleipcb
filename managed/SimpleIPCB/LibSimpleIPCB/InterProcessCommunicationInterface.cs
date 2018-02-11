using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;


namespace LibSimpleIPCB
{

	public class InterProcessCommunicationInterface
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		private static bool DEBUGGING_ENABLED = false;

		private static readonly int MAX_STDIN_CHUNK_SIZE = 65536;

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		private ReadWriteBuffer incomingBuffer;
		private ReadWriteBuffer outgoingBuffer;
		// private Thread readFromStdInThread;
		private Stream stdin;
		private Stream stdout;
		private Dictionary<string, StructPacketHandler> messageHandlers;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		public InterProcessCommunicationInterface()
		{
			stdin = Console.OpenStandardInput();
			stdout = Console.OpenStandardOutput();

			incomingBuffer = new ReadWriteBuffer();
			outgoingBuffer = new ReadWriteBuffer();

			messageHandlers = new Dictionary<string, StructPacketHandler>();
		}


		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		public void RegisterHandler(string targetID, Type argumentType, Type returnType, MessageHandlerDelegate handler)
		{
			if (messageHandlers.ContainsKey(targetID))
				throw new Exception("Handler already registered for '" + targetID + "'!");
			messageHandlers.Add(targetID, new StructPacketHandler(handler, argumentType, returnType));
		}

		public void Run()
		{
			// ThreadStart st = new ThreadStart(__ReadFromStdIn);
			// readFromStdInThread = new Thread(st);
			// readFromStdInThread.Start();
			__ReadFromStdInLoop();
		}

		private void __Debug(string text)
		{
			if (DEBUGGING_ENABLED) {
				File.AppendAllText("_debug_.txt", text + Environment.NewLine);
			}
		}

		/// <summary>
		/// This method is invoked in an own thread. This method tries to read data from STDIN and then tries to process it.
		/// </summary>
		private void __ReadFromStdInLoop()
		{
			while (true) {
				// read data from input stream
				__Debug("read data from input stream");
				incomingBuffer.EnsureFreeCapacity(MAX_STDIN_CHUNK_SIZE);
				int bytesRead = stdin.Read(incomingBuffer.Data, incomingBuffer.WritePos, MAX_STDIN_CHUNK_SIZE);
				if (bytesRead <= 0) {
					__Debug("Terminating.");
					break;
				}
				incomingBuffer.WritePos += bytesRead;

				// is there enough data available?
				__Debug("is there enough data available?");
				string targetID;
				byte[] rawPayload;
				if (!PacketDecoder.TryReadPacket(incomingBuffer, out targetID, out rawPayload)) {
					// not enough data
					__Debug("not enough data");
					continue;
				}

				// get message handler
				__Debug("get message handler");
				StructPacketHandler messageHandlerStruct;
				bool bSuccess;
				lock (this) {
					bSuccess = messageHandlers.TryGetValue(targetID, out messageHandlerStruct);
				}
				if (!bSuccess) {
					string errMsg = "ERROR: No message handler for target: \'" + targetID + "\'";
					__Debug(errMsg);
					__Debug("Message handlers available:");
					foreach (string key in messageHandlers.Keys) {
						__Debug("-- \"" + key + "\"");
					}
					throw new Exception(errMsg);
				}

				// parse arguments
				__Debug("parse arguments");
				AbstractDataPacket argumentData = (AbstractDataPacket)(Activator.CreateInstance(messageHandlerStruct.ArgumentDataType, rawPayload));

				// dispatch packet
				__Debug("dispatch packet");
				AbstractDataPacket retValue = messageHandlerStruct.Handler(argumentData);

				// verify return data
				__Debug("verify return data");
				if (retValue == null)
					retValue = (AbstractDataPacket)(Activator.CreateInstance(messageHandlerStruct.ReturnDataType));
				else
					if (retValue.GetType() != messageHandlerStruct.ReturnDataType) {
						string errMsg = "Data returned is of type '" + retValue.GetType().ToString() + "' and not of type '" + messageHandlerStruct.ReturnDataType.ToString() + "'!";
						__Debug("ERROR: " + errMsg);
						throw new Exception(errMsg);
					}

				// return data to sender
				__Debug("return data to sender");
				PacketEncoder.WritePacket(retValue.ToArray(), outgoingBuffer);
				__Debug(outgoingBuffer.WritePos + " bytes of data to send");
				stdout.Write(outgoingBuffer.Data, 0, outgoingBuffer.WritePos);
				stdout.Flush();
				outgoingBuffer.Clear();
			}
		}

	}

}

