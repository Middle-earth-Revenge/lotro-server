using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Protocol.Generic;
using System.IO;
using Helper;

namespace Protocol.SessionSetup.Session
{
    public class Terminate : PayloadData
    {

        public override object Deserialize(BEBinaryReader ber)
        {
            // no need to read

            return this;
        }

        // needs to be implemented
        public override byte[] Serialize(BEBinaryWriter beBinaryWriter)
        {
            byte[] rawPacket = null;



            return rawPacket;
        }

        public override UInt16 Request
        {
            get { return 8; }
        }

        public override byte[] Response
        {
            get { return new byte[] { 0x00, 0x00, 0x04, 0x00 }; }
        }
    }
}
