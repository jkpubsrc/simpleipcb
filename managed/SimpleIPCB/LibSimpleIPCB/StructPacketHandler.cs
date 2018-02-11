using System;


namespace LibSimpleIPCB
{

	public struct StructPacketHandler
	{

		public MessageHandlerDelegate Handler;
		public Type ArgumentDataType;
		public Type ReturnDataType;

		public StructPacketHandler(MessageHandlerDelegate handler, Type argumentDataType, Type returnDataType)
		{
			this.Handler = handler;
			this.ArgumentDataType = argumentDataType;
			this.ReturnDataType = returnDataType;
		}

	}

}

