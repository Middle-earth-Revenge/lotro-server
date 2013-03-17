using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using System.IO;

namespace Protocol.Data
{
    public class _00_01_00_00 : Protocol.Generic.ObjectData
    {
        private byte[] start = { 0x00, 0x01 };
        private byte[] data = {	0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00 };

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
