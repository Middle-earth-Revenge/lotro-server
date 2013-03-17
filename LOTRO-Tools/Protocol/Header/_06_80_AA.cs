﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using System.IO;

namespace Protocol.Header
{
    public class _06_80_AA:Protocol.Generic.ObjectHeader
    {

        private byte[] start = new byte[] { 0x01, 0x02, 0x02, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06 };

        public override byte[] ident {

            get { return new byte[] { 0x80, 0xAA }; }
        }

        public override object Deserialize(BEBinaryReader ber)
        {

            return null;
        }
        public override void Serialize(BEBinaryWriter beBinaryWriter)
        {
            beBinaryWriter.Write(start);
            beBinaryWriter.Write(ident);
            beBinaryWriter.Write(new byte[ident.Length]);

            beBinaryWriter.Flush();

        }
    }
}
