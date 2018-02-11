using System;


using LibSimpleIPCB;


namespace ExampleUTF8Echo
{

	class MainClass
	{

		public static void Main(string[] args)
		{
			InterProcessCommunicationInterface ipci = new InterProcessCommunicationInterface();
			ipci.RegisterHandler("echo-test", typeof(UTF8DataPacket), typeof(UTF8DataPacket), new MessageHandlerDelegate(HandleUTF8Echo));
			ipci.Run();
		}

		public static AbstractDataPacket HandleUTF8Echo(AbstractDataPacket pkg)
		{
			// do nothing in this specific case; just return the data received for test purposes;
			return pkg;
		}

	}

}

