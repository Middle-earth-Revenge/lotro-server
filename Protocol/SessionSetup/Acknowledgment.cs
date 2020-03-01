using System;
using System.IO;
using Helper;
using System.Text;
using System.Diagnostics;
using Protocol.Generic;

namespace Protocol.SessionSetup
{
    public class Acknowledgment : Protocol.Generic.PayloadData
    {
        public UInt64 StartupSessionKey { get; set; }
        public byte[] UnknownA { get; set; }
        public UInt32 UnknownB { get; set; }


        public override object Deserialize(BEBinaryReader ber)
        {
            try
            {
                byte[] reversedStartupSessionKey = ber.ReadBytes(8); // our startup session key is reversed in response
                Array.Reverse(reversedStartupSessionKey);
                StartupSessionKey = BitConverter.ToUInt64(reversedStartupSessionKey, 0);
                UnknownA = ber.ReadBytes(16); // the rest of this data seems to be static (in every online session the same)
                UnknownB = ber.ReadUInt32BE(); // changing, but what does it mean?
            }
            catch (EndOfStreamException eos)
            {
                Debug.WriteLine(eos);
            }
            catch (ObjectDisposedException od)
            {
                Debug.WriteLine(od);
            }
            catch (IOException io)
            {
                Debug.WriteLine(io);
            }

            return this;
        }

        public override byte[] Serialize(BEBinaryWriter beBinaryWriter)
        {
            // Here goes the new payload data handling with data segments inside payload data
            throw new NotImplementedException("This should not be called");
        }
        public override UInt16 Request { get { return 8; } } // to do

        public override byte[] Response
        {
            get { return new byte[] { 0x00, 0x00, 0x04, 0x00 }; }
        }
    }
}
