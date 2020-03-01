using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using System.IO;

namespace Protocol.Data
{
    public class _00_08 : Protocol.Generic.ObjectData
    {
        private byte[] start = { 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public byte[] data = { 0x9E,0x81,0xE5,0x0A};
        private byte[] end = { 0xFE,0x01,0x00,0x25, 0x01, 0x00, 0x00, 0x00, 0x00 };

        public override object Deserialize(BEBinaryReader ber)
        {
            // no need to read

            return this;
        }

        public override void Serialize(BEBinaryWriter beBinaryWriter)
        {           
            beBinaryWriter.Write(start);
            beBinaryWriter.Write(data);
            beBinaryWriter.Write(end);

            beBinaryWriter.Flush();
        }

    }
}
