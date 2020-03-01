using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketBrowser
{
    // Bit converter with endian support
    public static class CustomBitConverter
    {
        public static short ToInt16(byte[] data, int offset, bool bigEndian = true)
        {
            if (bigEndian)
            {
                byte[] buffer = new byte[2];

                // Big endian
                buffer[0] = data[offset + 1];
                buffer[1] = data[offset];

                return BitConverter.ToInt16(buffer, 0);
            }
            else
            {
                return BitConverter.ToInt16(data, offset);
            }
        }

        public static ushort ToUInt16(byte[] data, int offset, bool bigEndian = true)
        {
            if (bigEndian)
            {
                byte[] buffer = new byte[2];

                // Big endian
                buffer[0] = data[offset + 1];
                buffer[1] = data[offset];

                return BitConverter.ToUInt16(buffer, 0);
            }
            else
            {
                return BitConverter.ToUInt16(data, offset);
            }
        }

        public static int ToInt32(byte[] data, int offset, bool bigEndian = true)
        {
            if (bigEndian)
            {
                byte[] buffer = new byte[4];

                // Big endian
                buffer[0] = data[offset + 3];
                buffer[1] = data[offset + 2];
                buffer[2] = data[offset + 1];
                buffer[3] = data[offset];

                return BitConverter.ToInt32(buffer, 0);
            }
            else
            {
                return BitConverter.ToInt32(data, offset);
            }
        }

        public static uint ToUInt32(byte[] data, int offset, bool bigEndian = true)
        {
            if (bigEndian)
            {
                byte[] buffer = new byte[4];

                // Big endian
                buffer[0] = data[offset + 3];
                buffer[1] = data[offset + 2];
                buffer[2] = data[offset + 1];
                buffer[3] = data[offset];

                return BitConverter.ToUInt32(buffer, 0);
            }
            else
            {
                return BitConverter.ToUInt32(data, offset);
            }
        }

        public static long ToInt64(byte[] data, int offset, bool bigEndian = true)
        {
            if (bigEndian)
            {
                byte[] buffer = new byte[4];

                // Big endian
                buffer[0] = data[offset + 7];
                buffer[1] = data[offset + 6];
                buffer[2] = data[offset + 5];
                buffer[3] = data[offset + 4];
                buffer[4] = data[offset + 3];
                buffer[5] = data[offset + 2];
                buffer[6] = data[offset + 1];
                buffer[7] = data[offset];

                return BitConverter.ToInt64(buffer, 0);
            }
            else
            {
                return BitConverter.ToInt64(data, offset);
            }
        }

        public static ulong ToUInt64(byte[] data, int offset, bool bigEndian = true)
        {
            if (bigEndian)
            {
                byte[] buffer = new byte[4];

                // Big endian
                buffer[0] = data[offset + 7];
                buffer[1] = data[offset + 6];
                buffer[2] = data[offset + 5];
                buffer[3] = data[offset + 4];
                buffer[4] = data[offset + 3];
                buffer[5] = data[offset + 2];
                buffer[6] = data[offset + 1];
                buffer[7] = data[offset];

                return BitConverter.ToUInt64(buffer, 0);
            }
            else
            {
                return BitConverter.ToUInt64(data, offset);
            }
        }

        public static float ToSingle(byte[] data, int offset, bool bigEndian = true)
        {
            if (bigEndian)
            {
                byte[] buffer = new byte[4];

                // Big endian
                buffer[0] = data[offset + 3];
                buffer[1] = data[offset + 2];
                buffer[2] = data[offset + 1];
                buffer[3] = data[offset];

                return BitConverter.ToSingle(buffer, 0);
            }
            else
            {
                return BitConverter.ToSingle(data, offset);
            }
        }

        public static double ToDouble(byte[] data, int offset, bool bigEndian = true)
        {
            if (bigEndian)
            {
                byte[] buffer = new byte[4];

                // Big endian
                buffer[0] = data[offset + 7];
                buffer[1] = data[offset + 6];
                buffer[2] = data[offset + 5];
                buffer[3] = data[offset + 4];
                buffer[4] = data[offset + 3];
                buffer[5] = data[offset + 2];
                buffer[6] = data[offset + 1];
                buffer[7] = data[offset];

                return BitConverter.ToDouble(buffer, 0);
            }
            else
            {
                return BitConverter.ToDouble(data, offset);
            }
        }
    }
}
