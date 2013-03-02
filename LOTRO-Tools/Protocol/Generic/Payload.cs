using System;
using Protocol.Generic;
using System.Diagnostics;
using Helper;

/* Class PayloadPacket
 * 
 * Contains:
 * 
 * Payload header
 * Payload data (can contain zero til n protocol elements)
 * needs to be implemented, that means: before sender queue something must handle more elements inside + split up if length larger than 512 bytes
 * 
*/

namespace Protocol.Generic
{
    public abstract class Payload
    {       
        public virtual PayloadData Data { get; set; }

        public virtual PayloadHeader Header { get; set; }

        public abstract Payload Deserialize(BEBinaryReader ber);

        public abstract byte[] Serialize(BEBinaryWriter bew);
    }
}
