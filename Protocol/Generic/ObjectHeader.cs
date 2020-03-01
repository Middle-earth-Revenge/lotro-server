using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;

namespace Protocol.Generic
{
    public abstract class ObjectHeader
    {
        public UInt16 DataLength { get; set; }
        public abstract byte[] ident {get;}
        public UInt32 Checksum { get; set; }

        public abstract object Deserialize(BEBinaryReader ber);
        public abstract void Serialize(BEBinaryWriter beBinaryWriter);
    }
}
