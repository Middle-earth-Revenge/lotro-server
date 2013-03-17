using System;
using Helper;
using System.IO;
using System.Collections.Generic;

namespace Protocol.Generic
{
    public abstract class PayloadData
    {
        public PayloadData()
        {
            DataObjectList = new List<DataObject>();
        }

        public virtual List<DataObject> DataObjectList { get; set; }

        public UInt16 Length { get; set; }
        
        public abstract UInt16 Request { get;}
        public abstract byte[] Response { get; }

        public UInt32 Checksum { get; set; }
        public UInt32 XORValue { get; set; }

        public abstract object Deserialize(BEBinaryReader beBinaryReader);
        
        public virtual byte[] Serialize(BEBinaryWriter beBinaryWriter)
        {
            foreach(DataObject dataObject in DataObjectList)
            {
                dataObject.Serialize(beBinaryWriter);
                Checksum += dataObject.Checksum;
            }

            beBinaryWriter.Flush();

            byte[] rawBytes = ((MemoryStream)beBinaryWriter.BaseStream).ToArray();

            //Checksum = calcChecksum(rawBytes);
            Length = (UInt16)rawBytes.Length;

            beBinaryWriter.BaseStream.Position = 0;

            return rawBytes;
        }

        private UInt32 calcChecksum(byte[] rawBytes)
        {
            UInt32 checksum = Helper.HelperMethods.Instance.getChecksumFromData(rawBytes);

            return checksum;
        }
    }
}
