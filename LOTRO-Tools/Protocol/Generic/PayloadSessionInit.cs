using System;
using Helper;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace Protocol.Generic
{
    public class PayloadSessionInit: Payload
    {
        public PayloadSessionInit() : base()
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
            byte[] rawPayloadData = Data.Serialize(bew);

            Header.Action = Data.Response;
            Header.DataLength = Data.Length;
            Header.Checksum = Data.Checksum;

            byte[] rawPayloadHeader = Header.Serialize(bew);

            bew.WriteUInt16BE(0x00); // Unencrypted answer must start with 0x00
            bew.Write(rawPayloadHeader);
            bew.Write(rawPayloadData);

            bew.Flush();

            byte[] data = ((MemoryStream)bew.BaseStream).ToArray();

            byte[] dumpData = new byte[data.Length - 2];
            Buffer.BlockCopy(data, 2, dumpData, 0, data.Length - 2);

            string postfix = BitConverter.ToString(dumpData, 4, 4).Replace("-", "");

            Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, String.Format("{0,4:0000}", dumpCounter++) + "_server-" + postfix, new System.Text.UTF8Encoding().GetBytes(ToString(dumpData)), new System.Text.UTF8Encoding().GetBytes(ToString(dumpData)).Length, true);

            return data;
        }

        public override Payload Deserialize(BEBinaryReader ber)
        {
            Header = new PayloadHeader();
            Header.Deserialize(ber);

            if (validateChecksum(ber, Header))
            {
                Data = new ProtocolHandler().getPayloadSessionInit(Header.Action[1]);
                Data = (PayloadData)Data.Deserialize(ber);
            }
            else
            {
                Data = new WrongChecksum();
                Debug.WriteLineIf(Settings.Config.Instance.Debug, "Checksum for client packet is not correct.");
            }

            return this;
        }

        private static bool validateChecksum(BEBinaryReader ber, PayloadHeader Header)
        {
            long tempPosition = ber.BaseStream.Position;

            byte[] data = new byte[Header.DataLength];
            ber.Read(data, 0, Header.DataLength);

            Helper.Checksum chksum = new Checksum();
            UInt32 checksumData = chksum.generateChecksumFromData(data);

            // XXX WOW, this is just great! Calling this method reveses the Header.Action field ...
            byte[] actionCopy = new byte[Header.Action.Length];
            Buffer.BlockCopy(Header.Action, 0, actionCopy, 0, Header.Action.Length);
            UInt32 checksumHead = Helper.HelperMethods.Instance.generateChecksumForHeader(Header.SessionID, Header.DataLength, actionCopy /*Header.Action*/, Header.SequenceNumber, checksumData, Header.ACKNR);

            ber.BaseStream.Position = tempPosition;

            return (checksumHead == Header.Checksum) ? true : false;
        }

    }
}
