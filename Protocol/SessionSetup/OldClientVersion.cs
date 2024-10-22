﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Protocol.Generic;
using System.IO;
using Helper;

namespace Protocol.SessionSetup
{
    public class OldClientVersion : PayloadData
    {

       /* public override byte[] data
        {
            get { return new byte[] { 0x48, 0xE9, 0xA7, 0x00, 0x00, 0x00, 0x00, 0x00 }; }
        }*/

        public override object Deserialize(BEBinaryReader ber)
        {
            // no need to read

            return this;
        }

        public override byte[] Serialize(BEBinaryWriter beBinaryWriter)
        {
            beBinaryWriter.Write(new byte[] { 0x48, 0xE9, 0xA7, 0x00, 0x00, 0x00, 0x00, 0x00 }); 

            beBinaryWriter.Flush();

            byte[] rawBytes = ((MemoryStream)beBinaryWriter.BaseStream).ToArray();

            Length = (UInt16)rawBytes.Length;
            Checksum = Helper.HelperMethods.Instance.getChecksumFromData(rawBytes);

            beBinaryWriter.BaseStream.Position = 0;

            return rawBytes;
        }

        public OldClientVersion()
            : base()
        {
            
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
