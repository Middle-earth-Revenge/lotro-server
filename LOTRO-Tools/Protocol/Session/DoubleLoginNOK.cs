using System;
using Protocol.Generic;
using System.IO;
using Helper;
using System.Text;

namespace Protocol.Session
{
    public class DoubleLoginNOK : PayloadData
    {

        public DoubleLoginNOK()
        {
            data = new byte[] { 0x00 };
        }

        public override object Deserialize(BEBinaryReader ber)
        {
            // no need to read

            return this;
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
