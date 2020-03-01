using System.IO;
using System.Text;

namespace Helper
{
    public class BEBinaryReader : BinaryReader
    {
        public BEBinaryReader(Stream stream, Encoding encoding) : base(stream, encoding)
        {
        }

        public ulong ReadUInt64BE()
        {
            byte[] array = base.ReadBytes(8);
            ulong num = (ulong)(4294967295L & (array[7] | array[6] << 8 | array[5] << 16 | array[4] << 24));
            ulong num2 = (ulong)(4294967295L & (array[3] | array[2] << 8 | array[1] << 16 | array[0] << 24));
            return num2 << 32 | num;
        }

        public uint ReadUInt32BE()
        {
            uint num = base.ReadUInt32();
            return (num & 255u) << 24 | (num & 65280u) << 8 | (num & 16711680u) >> 8 | (num & 4278190080u) >> 24;
        }

        public ushort ReadUInt16BE()
        {
            ushort num = base.ReadUInt16();
            return (ushort)((num & 255) << 8 | (int)((uint)(num & 65280) >> 8));
        }

        public string ReadUnicodeString()
        {
            ushort num = ReadUnicodeStringLength();
            byte[] bytes = ReadBytes(num * 2);
            return Encoding.Unicode.GetString(bytes);
        }

        ushort ReadUnicodeStringLength()
        {
            byte b = ReadByte();
            ushort num;
            if (b > 128)
            {
                num = (ushort)(b + 127);
                b = ReadByte();
                num += b;
            }
            else if (b == 128)
            {
                num = ReadByte();
            }
            else
            {
                num = b;
            }
            return num;
        }
    }
}
