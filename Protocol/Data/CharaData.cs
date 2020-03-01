﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using System.IO;

namespace Protocol.Data
{
    public class CharaData : Protocol.Generic.ObjectData
    {

        public string PlainAccountName { get; set; }
        public string EncAccountName { get; set; }
        public string ClientIP { get; set; }

        private byte[] startPlainAccountName = { 0x00,0x00,0x00,0x00,0x00,0x0B,0x69,0xED,0x00,0x03,0xF7,0x01};
        private byte[] endPlainAccountName = { 0x01, 0x00, 0x00, 0x00, 0x00 };

        private byte[] startClientIP = { 0x08,0x2D,0x01 };
        private byte[] endClientIP = { 0x01, 0x00, 0x00, 0x00, 0x00 };

        private byte[] unknown = {
	        0x06, 0x6B, 0x04, 0x00, 0x00, 0x00, 0x06, 0x80, 0x02, 0x01, 0xA6, 0x02,
	        0x06, 0x2F, 0x00, 0x02, 0x06, 0x80, 0x02, 0x01, 0xA6, 0x06, 0x06, 0x2F,
	        0x10, 0x01, 0x06, 0x80, 0x02, 0x01, 0xA6, 0x01, 0x06, 0x2F, 0x10, 0x02,
	        0x06, 0x80, 0x02, 0x01, 0xA6, 0x14, 0x06, 0x2F, 0x00, 0x01 };

        private byte[] startEncAccountName = { 0x07, 0x47, 0x01 };
        private byte[] endEncAccountName = { 0x01, 0x00, 0x00, 0x00, 0x00 };

        byte[] unknown2 = {
	0x78, 0x85, 0x70, 0x44, 0x19, 0x00, 0x00, 0x00, 0x21, 0x02, 0x59, 0x8A,
	0x00, 0x01, 0xD6, 0x80, 0x9A, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x02, 0x01, 0xD7, 0x10, 0x04, 0x78, 0x66, 0x00, 0x01, 0x68, 0xFA, 0x01
};


        public override object Deserialize(BEBinaryReader ber)
        {
            // no need to read

            return this;
        }

        public override void Serialize(BEBinaryWriter beBinaryWriter)
        {
            beBinaryWriter.Write(startPlainAccountName);
            beBinaryWriter.WriteUnicodeString(PlainAccountName);
            beBinaryWriter.Write(endPlainAccountName);

            beBinaryWriter.Write(startClientIP);
            beBinaryWriter.WriteUnicodeString(ClientIP);
            beBinaryWriter.Write(endClientIP);

            beBinaryWriter.Write(unknown);

            beBinaryWriter.Write(startEncAccountName);
            beBinaryWriter.WriteUnicodeString(EncAccountName);
            beBinaryWriter.Write(endEncAccountName);

            beBinaryWriter.Write(unknown2);

            beBinaryWriter.Flush();
        }

    }
}