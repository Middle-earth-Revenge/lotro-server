using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;

namespace Protocol.Generic
{
    public abstract class ObjectData
    {
        public abstract object Deserialize(BEBinaryReader ber);
        public abstract void Serialize(BEBinaryWriter beBinaryWriter);
    }
}
