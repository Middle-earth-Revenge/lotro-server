using System;
using System.IO;
using Helper;
using System.Text;
using System.Diagnostics;
using Protocol.Generic;

namespace Protocol.SessionSetup
{
    public class SYNACK : Protocol.Generic.PayloadData
    {

        public UInt64 StartupSessionKey { get; set; }
        public UInt16 SessionID { get; set; }
        public UInt32 ChecksumServer { get; set; }
        public UInt32 ChecksumClient { get; set; }

        public override object Deserialize(BEBinaryReader ber)
        {
            throw new NotImplementedException("This should not be called");

         }

        public override byte[] Serialize(BEBinaryWriter beBinaryWriter)
        {
            beBinaryWriter.Write(new byte[] { 0x11, 0x11, 0x11, 0x11, 0x66, 0x66, 0x66, 0x66 }); // don't know maybe increasing ticks
            beBinaryWriter.Write(StartupSessionKey);
            beBinaryWriter.WriteUInt16BE(0x00); // Separator?
            beBinaryWriter.WriteUInt16BE(SessionID);
            beBinaryWriter.WriteUInt32BE(ChecksumServer);
            beBinaryWriter.WriteUInt32BE(ChecksumClient);
            beBinaryWriter.Write(new byte[] { 0xDA, 0x82, 0xCF, 0x61, 0x37, 0x12, 0x45, 0x85, 0x9C, 0x4E, 0x1C, 0x6D, 0x01, 0xF1, 0x62, 0xBB }); // const value, whatever?

            beBinaryWriter.Flush();

            byte[] rawBytes = ((MemoryStream)beBinaryWriter.BaseStream).ToArray();

            Length = (UInt16)rawBytes.Length;
            Checksum = Helper.HelperMethods.Instance.getChecksumFromData(rawBytes);

            beBinaryWriter.BaseStream.Position = 0;

            return rawBytes;
        }
        public override UInt16 Request { get { return 8; } } // to do

        public override byte[] Response
        {
            get { return new byte[] { 0x00, 0x04, 0x00, 0x00 }; }
        }
    }
}
