using System;
using Helper;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace Protocol.Generic
{
    public class PayloadSession: Payload
    {
        public PayloadSession()
            : base()
        {
            
        }

        public static string ToString(byte[] packet)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach (byte b in packet)
            {
                sb.AppendFormat("{0:x2}", b);
                count++;
                if (count % 2 == 0)
                {
                    sb.Append(' ');
                }
                if (count % 16 == 0 && count != packet.Length)
                {
                    sb.Append("\r\n");
                }
            }
            return sb.ToString();
        }

        public override byte[] Serialize(BEBinaryWriter bew, int dumpCounter)
        {
            MemoryStream ms = (MemoryStream)bew.BaseStream;
            ms.Position = 0;
            ms.SetLength(0);

            byte[] rawPayloadData = Data.Serialize(bew);

            Header.Action = Data.Response;
            Header.DataLength = Data.Length;
            Header.Checksum = Data.Checksum;

            if (Header.Action[3] == 0x06)
            {
                Header.Checksum = Data.Checksum ^ Data.XORValue;
            }

            byte[] rawPayloadHeader = Header.Serialize(bew);

            bew.Write(rawPayloadHeader);
            bew.Write(rawPayloadData);

            bew.Flush();

            byte[] data = ms.ToArray();

            string postfix = BitConverter.ToString(data, 4, 4).Replace("-", "");

            Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, String.Format("{0,4:0000}", dumpCounter++) + "_server-" + postfix, new System.Text.UTF8Encoding().GetBytes(ToString(data)), new System.Text.UTF8Encoding().GetBytes(ToString(data)).Length, true);

            Encrypt ep = new Encrypt();
            return ep.generateEncryptedPacket(data, false);
        }

        public override Payload Deserialize(BEBinaryReader ber)
        {
            Header = deserializeHeader(ber);         

            Data = deserializeData(ber);

            return this;
        }

        private PayloadHeader deserializeHeader(BEBinaryReader ber)
        {
            Header = new PayloadHeader();
            Header.Deserialize(ber);

            return Header;
        }

        private PayloadData deserializeData(BEBinaryReader ber)
        {
            Data = new ProtocolHandler().getPayloadSession(Header.Action);

            if(Data != null)
                Data = (PayloadData)Data.Deserialize(ber);

            return Data;
        }

        private bool validateChecksum(BEBinaryReader ber)
        {
            long tempPosition = ber.BaseStream.Position;

            byte[] data = new byte[Header.DataLength];
            ber.Read(data, 0, Header.DataLength);

            Helper.Checksum chksum = new Checksum();
            UInt32 checksumData = chksum.generateChecksumFromData(data);

            UInt32 checksumHead = Helper.HelperMethods.Instance.generateChecksumForHeader(Header.SessionID, Header.DataLength, Header.Action, Header.SequenceNumber, checksumData, Header.ACKNR);

            ber.BaseStream.Position = tempPosition;

            return (checksumHead == Header.Checksum) ? true : false;
        }

    }
}
