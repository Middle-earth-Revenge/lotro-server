using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using System.IO;

namespace Protocol.Data
{
    public class _00_78_66 : Protocol.Generic.ObjectData
    {
        private byte[] data = { 0x00,0x78,0x66,0x00,0x01};


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
