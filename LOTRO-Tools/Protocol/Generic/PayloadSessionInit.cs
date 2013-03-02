using System;
using Helper;
using System.IO;

namespace Protocol.Generic
{
    public class PayloadSessionInit: Payload
    {

        public PayloadSessionInit()
            : base()
        {

        }

        public override byte[] Serialize(BEBinaryWriter bew)
        {
            byte[] rawPayloadData = Data.Serialize(bew);

            Header.Action = Data.Response;
            Header.DataLength = Data.Length;
            Header.DataChecksum = Data.Checksum;

            byte[] rawPayloadHeader = Header.Serialize(bew);

            bew.WriteUInt16BE(0x00); // Unencrypted answer must start with 0x00
            bew.Write(rawPayloadHeader, 0, 0x14);
            bew.Write(rawPayloadData);

            bew.Flush();

            return ((MemoryStream)bew.BaseStream).ToArray();
        }

        public override Payload Deserialize(BEBinaryReader ber)
        {
            Header = deserializeHeader(ber);
            Data = dezerializeData(ber);

            return this;
        }

        private PayloadHeader deserializeHeader(BEBinaryReader ber)
        {
            Header = new PayloadHeader();
            Header.Deserialize(ber);

            return Header;
        }

        private PayloadData dezerializeData(BEBinaryReader ber)
        {
            Data = new ProtocolHandler().getPayloadSessionInit(Header.Action[1]);
            Data = (PayloadData)Data.Deserialize(ber);

            return Data;
        }

    }
}
