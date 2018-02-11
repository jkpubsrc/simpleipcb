using System;


using LibSimpleIPCB;



namespace TestIPCB
{

	public class MainClass
	{

		static byte[] REQUEST = new byte[]{
			0x13, 0x00, 0x00, 0x00,
			0x04, 0x65, 0x63, 0x68, 0x6f,
			0x0a, 0x00, 0x00, 0x00, 0x7b, 0x22, 0x6b, 0x22, 0x3a, 0x20, 0x22, 0x76, 0x22, 0x7d,
			0x13, 0x00, 0x00, 0x00,
			0x04, 0x65, 0x63, 0x68, 0x6f,
			0x0a, 0x00, 0x00, 0x00, 0x7b, 0x22, 0x6b, 0x22, 0x3a, 0x20, 0x22, 0x76, 0x22, 0x7d,
		};

		public static void Main(string[] args)
		{
			ReadWriteBuffer buffer = new ReadWriteBuffer();
			buffer.WriteBytes(REQUEST);

			string targetID;
			byte[] rawData;
			bool b = PacketDecoder.TryReadPacket(buffer, out targetID, out rawData);
			Console.WriteLine(b);
			Console.WriteLine(targetID);
			Console.WriteLine(rawData);
			b = PacketDecoder.TryReadPacket(buffer, out targetID, out rawData);
			Console.WriteLine(b);
			Console.WriteLine(targetID);
			Console.WriteLine(rawData);
		}

	}

}
