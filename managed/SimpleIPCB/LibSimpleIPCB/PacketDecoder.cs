using System;
using System.Collections.Generic;


namespace LibSimpleIPCB
{

	public class PacketDecoder
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Methods
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Static Methods
		////////////////////////////////////////////////////////////////

		public static bool TryReadPacket(ReadWriteBuffer buffer, out string targetID, out byte[] rawData)
		{
			int bytesAvailable = buffer.BytesAvailable;
			if (bytesAvailable < 4) {
				// not enough data
				targetID = null;
				rawData = null;
				return false;
			}
			int pos = buffer.ReadPos;
			int? pkgLength = buffer.PeekInt32(ref pos);
			if (bytesAvailable - 4 < pkgLength.Value) {
				// not enough data
				targetID = null;
				rawData = null;
				return false;
			}

			buffer.ReadPos += 4;
			DecodePacket(buffer, pkgLength.Value, out targetID, out rawData);
			return true;
		}

		public static void DecodePacket(ReadWriteBuffer buffer, int pkgLength, out string targetID, out byte[] rawData)
		{
			int pos = buffer.ReadPos;
			targetID = buffer.PeekStringU8(ref pos);
			rawData = buffer.PeekArray32(ref pos);
			int encounteredPkgLength = pos - buffer.ReadPos;
			if (encounteredPkgLength > pkgLength)
				throw new Exception("Packet content larger than package itself! pkgLength: "
					+ pkgLength + ", encounteredPkgLength=" + encounteredPkgLength);
			buffer.ReadPos += pkgLength;
			buffer.DiscardOldData();
		}

	}

}

