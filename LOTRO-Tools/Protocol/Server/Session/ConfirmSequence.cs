using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Protocol.Generic;
using System.IO;
using Helper;

namespace Protocol.Server.Session
{


    // only for 40

    public class ConfirmSequence : PayloadData
    {

        public UInt32 sequenceNr { get; set; }

        public override object Deserialize(BEBinaryReader ber)
        {
            sequenceNr = ber.ReadUInt32BE();

            return this;
        }

        public override byte[] Serialize(BEBinaryWriter beBinaryWriter)
        {
            beBinaryWriter.WriteUInt32BE(sequenceNr);
            beBinaryWriter.Flush();

            byte[] rawBytes = ((MemoryStream)beBinaryWriter.BaseStream).ToArray();

            Length = (UInt16)rawBytes.Length;
            Checksum = Helper.HelperMethods.Instance.getChecksumFromData(rawBytes);

            beBinaryWriter.BaseStream.Position = 0;

            return rawBytes;
        }       

        public override UInt16 Request
        {
            get { return 8; }
        }

        public override byte[] Response
        {
            get { return new byte[] { 0x00, 0x00, 0x40, 0x00 }; }
        }
    }
}
