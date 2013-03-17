using System;
using Helper;
using System.IO;

namespace Protocol.Generic
{
    public class DataObject
    {
        public virtual ObjectData Data { get; set; }
        public virtual ObjectHeader Header { get; set; }

        public UInt16 Length { get; set; }
        public UInt32 Checksum { get; set; }

        //public object Deserialize(BEBinaryReader beBinaryReader); // need to implemented
        /*public void Serialize(BEBinaryWriter beBinaryWriter)
        {
            MemoryStream ms = (MemoryStream)beBinaryWriter.BaseStream;
            
            long tempPosStart = ms.Position;
            Data.Serialize(beBinaryWriter);
            long tempPosEnd = ms.Position;

            byte[] data = getRawData((int)tempPosStart, (int)tempPosEnd, ms);
            Checksum = Helper.HelperMethods.Instance.getChecksumFromData(data);

            Header.DataLength = (UInt16)data.Length; // important
            tempPosStart = ms.Position;
            Header.Serialize(beBinaryWriter);
            tempPosEnd = ms.Position;

            byte[] header = getRawData((int)tempPosStart, (int)tempPosEnd, ms);
            Checksum += Helper.HelperMethods.Instance.getChecksumFromData(header);

            beBinaryWriter.BaseStream.Flush();
        }*/
        public void Serialize(BEBinaryWriter beBinaryWriter)
        {
            MemoryStream ms = (MemoryStream)beBinaryWriter.BaseStream;

            long startHeader = ms.Position;
            Header.Serialize(beBinaryWriter);
            long endHeader = ms.Position;

            long startData = ms.Position;
            Data.Serialize(beBinaryWriter);
            long endData = ms.Position;

            byte[] data = getRawData((int)startData, (int)endData, ms);
            Checksum = Helper.HelperMethods.Instance.getChecksumFromData(data);

            ms.Position = endHeader - Header.ident.Length;
            beBinaryWriter.WriteUInt16BEX((UInt16)data.Length);

            byte[] header = getRawData((int)startHeader, (int)endHeader, ms);
            Checksum += Helper.HelperMethods.Instance.getChecksumFromData(header);

            beBinaryWriter.BaseStream.Flush();

            ms.Position = endData;
        }

        private byte[] getRawData(int start,int end, MemoryStream ms)
        {
            int byteCount = end - start;
            ms.Position = start;

            byte[] data = new byte[byteCount];

            ms.Read(data, 0, byteCount);

            return data;
        }
    }
}
