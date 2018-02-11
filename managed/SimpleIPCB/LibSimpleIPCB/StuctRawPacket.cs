using System;


namespace LibSimpleIPCB
{

	public struct StuctIncomingPacket
	{

		public string TargetID;
		public AbstractDataPacket Payload;

		public StuctIncomingPacket(string targetID, AbstractDataPacket payload)
		{
			this.TargetID = targetID;
			this.Payload = payload;
		}
	}

}

