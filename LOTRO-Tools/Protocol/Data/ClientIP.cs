using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using System.IO;

namespace Protocol.Data
{
    public class ClientIP : Protocol.Generic.ObjectData
    {

        public string ClientIPAddress { get; set; }

        private byte[] start = { 0x00, 0x08, 0x2D, 0x01 }; // most unicode strings start with an 01
        private byte[] end = { 0x01, 0x00, 0x00, 0x00, 0x00 }; // most strings end with this

        public override object Deserialize(BEBinaryReader ber)
        {
            // no need to read

            return this;
        }

        public override void Serialize(BEBinaryWriter beBinaryWriter)
        {           
            beBinaryWriter.Write(start);
            beBinaryWriter.WriteUnicodeString(ClientIPAddress);
            beBinaryWriter.Write(end);

            beBinaryWriter.Flush();
        }

    }
}
