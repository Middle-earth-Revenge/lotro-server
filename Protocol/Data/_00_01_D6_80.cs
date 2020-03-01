using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using System.IO;

namespace Protocol.Data
{
    public class _00_01_D6_80 : Protocol.Generic.ObjectData
    {
        private byte[] start = { 0x00, 0x01 };
        private byte[] data = {	0xD6, 0x80, 0x9A, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 };

        public override object Deserialize(BEBinaryReader ber)
        {
            // no need to read

            return this;
        }

        public override void Serialize(BEBinaryWriter beBinaryWriter)
        {           
            beBinaryWriter.Write(start);
            beBinaryWriter.Write(data);

            beBinaryWriter.Flush();
        }

    }
}
