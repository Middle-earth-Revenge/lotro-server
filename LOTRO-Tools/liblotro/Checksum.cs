using System;

namespace Helper
{
    public class Checksum
    {
        public uint[] updateChecksumTable()
        {
            return new uint[0];
        }
        public uint[] generateInitialChecksumTable(uint value)
        {
            return new uint[0];
        }
        public uint generateChecksumFromHead(byte[] header)
        {
            return 0;
        }

		public uint generateDefaultHeadChecksum(byte[] header)
        {
            return 0;
        }

        public uint generateChecksumFromData(byte[] data)
        {
            return 0;
        }
    }
}
