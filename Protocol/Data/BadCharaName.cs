using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using System.IO;

namespace Protocol.Data
{
    public class BadCharaName : Protocol.Generic.ObjectData
    {

        public byte[] start = { 0x00, 0x0A,0x00,0xDB,0x8C};

        public override object Deserialize(BEBinaryReader ber)
        {
            // no need to read

            return this;
        }

        public override void Serialize(BEBinaryWriter beBinaryWriter)
        {           
            beBinaryWriter.Write(start);
            beBinaryWriter.Flush();
        }

    }
}
