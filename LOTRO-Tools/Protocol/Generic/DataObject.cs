using System;
using Helper;

namespace Protocol.Generic
{
    public abstract class DataObject
    {
        public UInt16 Length { get; set; }
        public UInt32 Checksum { get; set; }

        public byte[] Identification { get; set; }

        public abstract object Deserialize(BEBinaryReader beBinaryReader);
        public abstract byte[] Serialize(BEBinaryWriter beBinaryWriter);
    }
}
