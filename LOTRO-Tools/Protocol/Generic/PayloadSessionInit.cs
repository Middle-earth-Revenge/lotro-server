using System;
using Helper;
using System.IO;
using System.Diagnostics;

namespace Protocol.Generic
{
    public class PayloadSessionInit: Payload
    {

        //private BEBinaryReader ber = null;

        public PayloadSessionInit()
            : base()
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
            Header = deserializeHeader(ber);         

            if (validateChecksum(ber))
            {
                Data = deserializeData(ber);
            }
            else
            {
                Data = new WrongChecksum();
                Debug.WriteLineIf(Settings.Config.Instance.Debug, "Checksum for client packet is not correct.");
            }

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
            Data = new ProtocolHandler().getPayloadSessionInit(Header.Action[1]);
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
