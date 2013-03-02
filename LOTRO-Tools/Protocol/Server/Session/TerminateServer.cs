﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Protocol.Generic;
using System.IO;
using Helper;

namespace Protocol.Server.Session
{
    public class TerminateServer : PayloadData
    {

        public TerminateServer()
        {
            data = new byte[] { 0x6E, 0x25, 0xFB, 0x0D, 0x00, 0x00, 0x00, 0x00 };
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
            get { return new byte[] { 0x00, 0x10, 0x00, 0x00 }; }
        }
    }
}
