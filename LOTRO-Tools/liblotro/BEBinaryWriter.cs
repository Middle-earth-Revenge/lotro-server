using System;
using System.IO;
using System.Text;

namespace Helper
{
    public class BEBinaryWriter : BinaryWriter
    {
        public BEBinaryWriter(Stream stream, Encoding encoding) : base(stream, encoding)
        {
        }

        public void WriteUInt64BE(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes, 0, 8);
            Write(bytes);
        }

        public void WriteUInt32BE(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes, 0, 4);
            Write(bytes);
        }

        public void WriteUInt16BE(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes, 0, 2);
            Write(bytes);
        }

        public void WriteUnicodeString(string value)
        {
            // TODO
        }

    }
}
