using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using System.IO;

namespace Protocol.Data
{
    public class ServerName : Protocol.Generic.ObjectData
    {

        public string Server { get; set; }

        private byte[] start = { 0x00, 0x01 };
        private byte[] end = { 0x01, 0x00, 0x00, 0x00, 0x00 };

        public override object Deserialize(BEBinaryReader ber)
        {
            // no need to read

            return this;
        }

        public override void Serialize(BEBinaryWriter beBinaryWriter)
        {           
            beBinaryWriter.Write(start);
            beBinaryWriter.WriteUnicodeString(Server);
            beBinaryWriter.Write(end);

            beBinaryWriter.Flush();
        }

    }
}
