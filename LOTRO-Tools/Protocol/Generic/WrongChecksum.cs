using System;
using Helper;

namespace Protocol.Generic
{

        public class WrongChecksum : Protocol.Generic.PayloadData
        {

            public override object Deserialize(BEBinaryReader ber)
            {
                return null;
            }

            public override byte[] Serialize(BEBinaryWriter beBinaryWriter)
            {
                return null;
            }

            public override UInt16 Request { get { return 0; } } 

            public override byte[] Response
            {
                get { return new byte[] { 0x00, 0x00, 0x00, 0x00 }; }
            }
        }
    }

