using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using System.IO;

namespace Protocol.Data
{
    public class _00_06_6B : Protocol.Generic.ObjectData
    {
        private byte[] data = { 0x00,0x06,0x6B,0x01,0x00,0x00,0x00,0x06,0x80,0x02,0x01,0xA6,0x02,0x06,0x2F,0x00,0x02};


        public override object Deserialize(BEBinaryReader ber)
        {
            // no need to read

            return this;
        }

        public override void Serialize(BEBinaryWriter beBinaryWriter)
        {           
            beBinaryWriter.Write(data);

            beBinaryWriter.Flush();
        }

    }
}
