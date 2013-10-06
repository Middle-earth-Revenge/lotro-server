using System;
using Helper;
using System.IO;
using System.Diagnostics;

namespace Protocol.Generic
{
    public class PayloadSessionInit: Payload
    {
        public PayloadSessionInit() : base()
        {
        }

        public override byte[] Serialize(BEBinaryWriter bew)
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

            return ((MemoryStream)bew.BaseStream).ToArray();
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
