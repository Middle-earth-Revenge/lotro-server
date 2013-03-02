using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helper;
using System.IO;

namespace Protocol._1ServerPacket
{
    public class _1224:Protocol.Generic.DataObject
    {
        public string SessionIPAddress { get; set; }

        public override object Deserialize(BEBinaryReader ber)
        {
            // no need to read

            return this;
        }

        // needs to be implemented
        public override byte[] Serialize(BEBinaryWriter beBinaryWriter)
        {

            byte[] startData = { 0x01, 0x02, 0x02, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06 };
            byte[] instruction = { 0x81, 0x49 };

            byte[] beginning = { 0x08, 0x2D, 0x01 };
            byte[] ipAddressLength = BitConverter.GetBytes(Encoding.UTF8.GetBytes(SessionIPAddress).Length);
            byte[] ipAddress = Encoding.Unicode.GetBytes(SessionIPAddress);
            byte[] ending = { 0x01, 0x00, 0x00, 0x00, 0x00 };

            byte[] lengthData = BitConverter.GetBytes((UInt16)(beginning.Length + ipAddressLength.Length + ipAddress.Length + ending.Length));

            beBinaryWriter.Write(startData);
            beBinaryWriter.Write(instruction);
            beBinaryWriter.Write(lengthData);
            beBinaryWriter.Write(beginning);
            beBinaryWriter.Write(ipAddressLength);
            beBinaryWriter.Write(ipAddress);
            beBinaryWriter.Write(ending);

            Length += (UInt16)ipAddress.Length;
            Checksum += Helper.HelperMethods.Instance.getChecksumFromData(ipAddress);

            byte[] rawBytes = ((MemoryStream)beBinaryWriter.BaseStream).ToArray();


            beBinaryWriter.BaseStream.Position = 0; // 3698B1B

            // // 099F9AAF

            return rawBytes;
        }

    }
}
