using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Helper;
using Protocol.Generic;

namespace Protocol.Session
{
    public class MultiDataObject : Protocol.Generic.PayloadData
    {
        public override object Deserialize(BEBinaryReader ber)
        {
            throw new NotImplementedException("This should not be called");
        }

        public override byte[] Serialize(BEBinaryWriter beBinaryWriter)
        {
            foreach (DataObject dataObject in DataObjectList)
            {
                dataObject.Serialize(beBinaryWriter);
                Checksum += dataObject.Checksum;
            }

            byte[] rawBytes = ((MemoryStream)beBinaryWriter.BaseStream).ToArray();

            Length = (UInt16)rawBytes.Length;

            beBinaryWriter.BaseStream.Position = 0;

            return rawBytes;
        }
        public override UInt16 Request { get { return 8; } } // to do

        public override byte[] Response
        {
            get { return new byte[] { 0x00, 0x00, 0x00, 0x06 }; }
        }
    }
}
