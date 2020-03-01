using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using System.IO;

namespace Protocol.Data
{
    public class AccountName : Protocol.Generic.ObjectData
    {

        public string Account { get; set; }

        private byte[] start = { 0x00,0x03,0xF7,0x01 };
        private byte[] end = { 0x01, 0x00, 0x00, 0x00, 0x00 };

        public override object Deserialize(BEBinaryReader ber)
        {
            // no need to read

            return this;
        }

        public override void Serialize(BEBinaryWriter beBinaryWriter)
        {           
            beBinaryWriter.Write(start);
            beBinaryWriter.WriteUnicodeString(Account);
            beBinaryWriter.Write(end);

            beBinaryWriter.Flush();
        }

    }
}
