using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using System.IO;

namespace Protocol.Data
{
    public class EncAccountName : Protocol.Generic.ObjectData
    {

        public string EncAccount { get; set; }

        private byte[] start = { 0x00,0x94,0xF9,0xE5,0x08};
        private byte[] end = { 0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00 };

        public override object Deserialize(BEBinaryReader ber)
        {
            // no need to read

            return this;
        }

        public override void Serialize(BEBinaryWriter beBinaryWriter)
        {           
            beBinaryWriter.Write(start);
            beBinaryWriter.WriteUnicodeString(EncAccount);
            beBinaryWriter.Write(end);

            beBinaryWriter.Flush();
        }

    }
}
