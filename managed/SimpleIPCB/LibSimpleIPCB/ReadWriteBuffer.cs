using System;
using System.Collections.Generic;
using System.Text;


namespace LibSimpleIPCB
{

	/// <summary>
	/// This class implements a dynamic buffer.
	/// </summary>
	public class ReadWriteBuffer
	{

		////////////////////////////////////////////////////////////////
		// Constants
		////////////////////////////////////////////////////////////////

		////////////////////////////////////////////////////////////////
		// Variables
		////////////////////////////////////////////////////////////////

		int writePos;
		int readPos;
		byte[] buffer;

		////////////////////////////////////////////////////////////////
		// Constructors
		////////////////////////////////////////////////////////////////

		/// <summary>
		/// Default constructor.
		/// </summary>
		public ReadWriteBuffer()
			: base()
		{
			buffer = new byte[65536];
		}

		/// <summary>
		/// Constructs a dynamic buffer using the specified initial capacity.
		/// </summary>
		public ReadWriteBuffer(int ensureCapacity)
		{
			int n = 16;
			while (n < ensureCapacity)
				n *= 2;
			buffer = new byte[n];
		}

		////////////////////////////////////////////////////////////////
		// Properties
		////////////////////////////////////////////////////////////////

		/// <summary>
		/// Direct access to the internal data buffer. Be aware that this buffer may be larger than
		/// the value indicated by <code>Count</code>.
		/// </summary>
		public byte[] Data
		{
			get {
				return buffer;
			}
		}

		public int ReadPos
		{
			get {
				return readPos;
			}
			set {
				if (value < 0) value = 0;
				else
					if (value > writePos) value = writePos;

				this.readPos = value;
			}
		}

		public int WritePos
		{
			get {
				return writePos;
			}
			set {
				if (value < 0) value = 0;
				else
					if (value > buffer.Length) value = buffer.Length;

				this.writePos = value;
			}
		}

		public int BytesAvailable
		{
			get {
				return writePos - readPos;
			}
		}

		////////////////////////////////////////////////////////////////
		// Helper Methods
		////////////////////////////////////////////////////////////////

		public void EnsureFreeCapacity(int requiredAmountOfSpace)
		{
			if (writePos + requiredAmountOfSpace <= buffer.Length)
				return;

			int n = buffer.Length * 2;
			while (writePos + requiredAmountOfSpace > n)
				n *= 2;

			byte[] newBuffer = new byte[n];
			Buffer.BlockCopy(buffer, 0, newBuffer, 0, writePos);
			buffer = newBuffer;
		}

		private static int __ReadInt32(byte[] buffer, ref int ofs, out int ret)
		{
			ret = ((int)buffer[ofs]) | (((int)buffer[ofs + 1]) << 8)
				| (((int)buffer[ofs + 2]) << 16) | (((int)buffer[ofs + 3]) << 24);
			ofs += 4;
			return 4;
		}

		private static int __ReadInt64(byte[] buffer, ref int ofs, out long ret)
		{
			ret = ((long)buffer[ofs]) | (((long)buffer[ofs + 1]) << 8)
				| (((long)buffer[ofs + 2]) << 16) | (((long)buffer[ofs + 3]) << 24)
				| (((long)buffer[ofs + 4]) << 32) | (((long)buffer[ofs + 5]) << 40)
				| (((long)buffer[ofs + 6]) << 48) | (((long)buffer[ofs + 7]) << 56);
			ofs += 8;
			return 8;
		}

		private static int __WriteInt32(byte[] buffer, ref int ofs, int value)
		{
			buffer[ofs++] = (byte)value;
			buffer[ofs++] = (byte)(value >> 8);
			buffer[ofs++] = (byte)(value >> 16);
			buffer[ofs++] = (byte)(value >> 24);
			return 4;
		}

		private static int __WriteInt64(byte[] buffer, ref int ofs, long value)
		{
			int value1 = (int)value;
			int value2 = (int)(value >> 32);
			buffer[ofs++] = (byte)value1;
			buffer[ofs++] = (byte)(value1 >> 8);
			buffer[ofs++] = (byte)(value1 >> 16);
			buffer[ofs++] = (byte)(value1 >> 24);
			buffer[ofs++] = (byte)value2;
			buffer[ofs++] = (byte)(value2 >> 8);
			buffer[ofs++] = (byte)(value2 >> 16);
			buffer[ofs++] = (byte)(value2 >> 24);
			return 8;
		}

		////////////////////////////////////////////////////////////////
		// Public Methods
		////////////////////////////////////////////////////////////////

		// ----------------------------------------------------------------
		// Peek Methods
		// ----------------------------------------------------------------

		public int? PeekInt32(ref int readPos)
		{
			int bytesAvailable = writePos - readPos;
			if (bytesAvailable < 4)
				return null;
			int ret;
			__ReadInt32(buffer, ref readPos, out ret);
			return ret;
		}

		public long? PeekInt64(ref int readPos)
		{
			int bytesAvailable = writePos - readPos;
			if (bytesAvailable < 8)
				return null;
			long ret;
			__ReadInt64(buffer, ref readPos, out ret);
			return ret;
		}

		public byte[] PeekBytes(ref int readPos, int len)
		{
			int bytesAvailable = writePos - readPos;
			if (bytesAvailable < len)
				return null;
			byte[] ret = new byte[len];
			Buffer.BlockCopy(buffer, readPos, buffer, 0, len);
			readPos += len;
			return ret;
		}

		public string PeekASCII(ref int readPos, int len)
		{
			int bytesAvailable = writePos - readPos;
			if (bytesAvailable < len)
				return null;

			string ret = Encoding.ASCII.GetString(buffer, readPos, len);
			readPos += len;
			return ret;
		}

		public string PeekStringU8(ref int readPos)
		{
			int bytesAvailable = writePos - readPos;
			if (bytesAvailable == 0)
				return null;
			int len = buffer[readPos];
			if (len == 0) {
				readPos++;
				return string.Empty;
			}
			if (bytesAvailable < len + 1)
				throw new Exception("Not enough data available: " + (len + 1) + " bytes required, " + bytesAvailable + " bytes available!");

			string ret = Encoding.UTF8.GetString(buffer, readPos + 1, len);
			readPos += len + 1;
			return ret;
		}

		public byte[] PeekArray32(ref int readPos)
		{
			int bytesAvailable = writePos - readPos;
			if (bytesAvailable < 4)
				return null;
			int len;
			__ReadInt32(buffer, ref readPos, out len);
			if (len < 0)
				return null;
			if (bytesAvailable < len + 4)
				throw new Exception("Not enough data available: " + (len + 4) + " bytes required, " + bytesAvailable + " bytes available!");

			byte[] ret = new byte[len];
			Buffer.BlockCopy(buffer, readPos, ret, 0, len);
			readPos += len;
			return ret;
		}

		// ----------------------------------------------------------------
		// Read Methods
		// ----------------------------------------------------------------

		public int ReadInt32()
		{
			int bytesAvailable = writePos - readPos;
			if (bytesAvailable < 4)
				throw new Exception("Not enough data available: 4 bytes expected, " + bytesAvailable + " bytes available!");
			int ret;
			__ReadInt32(buffer, ref readPos, out ret);
			return ret;
		}

		public long ReadInt64()
		{
			int bytesAvailable = writePos - readPos;
			if (bytesAvailable < 8)
				throw new Exception("Not enough data available: 4 bytes expected, " + bytesAvailable + " bytes available!");
			long ret;
			__ReadInt64(buffer, ref readPos, out ret);
			return ret;
		}

		public byte[] ReadBytes(int len)
		{
			int bytesAvailable = writePos - readPos;
			if (len > bytesAvailable)
				throw new Exception("Not enough data available: " + len + " bytes expected, " + bytesAvailable + " bytes available!");

			byte[] ret = new byte[len];
			Buffer.BlockCopy(buffer, readPos, buffer, 0, len);
			readPos += len;
			return ret;
		}

		public string ReadASCII(int len)
		{
			int bytesAvailable = writePos - readPos;
			if (len > bytesAvailable)
				throw new Exception("Not enough data available: " + len + " bytes expected, " + bytesAvailable + " bytes available!");

			string ret = Encoding.ASCII.GetString(buffer, readPos, len);
			readPos += len;
			return ret;
		}

		public string ReadStringU8()
		{
			int bytesAvailable = writePos - readPos;
			if (bytesAvailable == 0)
				throw new Exception("Not enough data available!");
			int len = buffer[readPos];
			if (len == 0) {
				readPos++;
				return string.Empty;
			}
			if (bytesAvailable < len + 1)
				throw new Exception("Not enough data available: " + (len + 1) + " bytes required, " + bytesAvailable + " bytes available!");

			string ret = Encoding.UTF8.GetString(buffer, readPos + 1, len);
			readPos += len + 1;
			return ret;
		}

		public byte[] ReadArray32(ref int readPos)
		{
			int bytesAvailable = writePos - readPos;
			if (bytesAvailable < 4)
				throw new Exception("Not enough data available!");
			int len;
			__ReadInt32(buffer, ref readPos, out len);
			if (len < 0)
				return null;
			if (bytesAvailable < len + 4)
				throw new Exception("Not enough data available: " + (len + 4) + " bytes required, " + bytesAvailable + " bytes available!");

			byte[] ret = new byte[len];
			Buffer.BlockCopy(buffer, readPos, ret, 0, len);
			readPos += len;
			return ret;
		}

		// ----------------------------------------------------------------
		// Write Methods
		// ----------------------------------------------------------------

		public void WriteInt64(int value)
		{
			EnsureFreeCapacity(4);
			__WriteInt32(buffer, ref writePos, value);
		}

		public void WriteInt32(int value)
		{
			EnsureFreeCapacity(4);
			__WriteInt32(buffer, ref writePos, value);
		}

		/// <summary>
		/// Append an element to the end of the buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteByte(byte value)
		{
			EnsureFreeCapacity(1);
			buffer[writePos] = value;
			writePos++;
		}

		/// <summary>
		/// Append an element to the end of the buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteBytes(byte[] values, int ofs, int len)
		{
			EnsureFreeCapacity(len);
			Buffer.BlockCopy(values, ofs, buffer, writePos, len);
			writePos += len;
		}

		/// <summary>
		/// Append an element to the end of the buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteBytes(byte[] values)
		{
			EnsureFreeCapacity(values.Length);
			Buffer.BlockCopy(values, 0, buffer, writePos, values.Length);
			writePos += values.Length;
		}

		public void WriteASCII(string data)
		{
			byte[] values = Encoding.ASCII.GetBytes(data);
			EnsureFreeCapacity(values.Length);
			Buffer.BlockCopy(values, 0, buffer, writePos, values.Length);
			writePos += values.Length;
		}

		public void WriteStringU8(string data)
		{
			if (data == null) {
				throw new Exception("Null can not be written!");
			} else { 
				byte[] values = Encoding.UTF8.GetBytes(data);
				if (values.Length > 255)
					throw new Exception("String too long: " + values.Length + " bytes");
				EnsureFreeCapacity(values.Length + 1);
				buffer[writePos++] = (byte)(values.Length);
				Buffer.BlockCopy(values, 0, buffer, writePos, values.Length);
				writePos += values.Length;
			}
		}

		public void WriteArray32(byte[] data)
		{
			if (data == null) {
				EnsureFreeCapacity(4);
				__WriteInt32(buffer, ref writePos, -1);
			} else {
				EnsureFreeCapacity(data.Length + 4);
				__WriteInt32(buffer, ref writePos, data.Length);
				Buffer.BlockCopy(data, 0, buffer, writePos, data.Length);
				writePos += data.Length;
			}
		}

		// ----------------------------------------------------------------
		// Other Methods
		// ----------------------------------------------------------------

		public void DiscardOldData()
		{
			if (readPos == 0) return;
			Buffer.BlockCopy(buffer, readPos, buffer, 0, writePos - readPos);
			writePos -= readPos;
			readPos = 0;
		}

		/*
		/// <summary>
		/// Compact the internal array by shrinking it to a suitable size.
		/// (The length of the internal array will be a multiple of 2 and at least 16 elements long.)
		/// </summary>
		public void Compact()
		{
			int n = buffer.Length;
			while ((n > 16) && (n > count)) {
				n >>= 1;
			}
			if (n != buffer.Length) {
				byte[] newBuffer = new byte[n];
				for (int i = 0; i < count; i++) {
					newBuffer[i] = buffer[i];
				}
				buffer = newBuffer;
			}
		}
		*/

		/// <summary>
		/// Remove all elements by simply setting <code>Count</code> to zero.
		/// The internal array is not modified.
		/// </summary>
		public void Clear()
		{
			readPos = 0;
			writePos = 0;
		}

		/// <summary>
		public byte[] ToArray()
		{
			int bytesAvailable = writePos - readPos;
			byte[] ret = new byte[bytesAvailable];
			Buffer.BlockCopy(buffer, readPos, ret, 0, bytesAvailable);
			return ret;
		}

	}

}
