using System;
using Helper;
using System.IO;
using System.Diagnostics;

namespace Protocol.Generic
{
    public class PayloadSession: Payload
    {
        public PayloadSession()
            : base()
        {
            
        }

        public override byte[] Serialize(BEBinaryWriter bew)
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

            byte[] rawData = ms.ToArray();

            //Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, DateTime.Now.ToLongTimeString().Replace(':','-') + "-unenc-send" , rawData, rawData.Length, true);

            FileStream fs = null;

            fs = new FileStream(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder + "\\" + DateTime.Now.Ticks + "-unenc-send", FileMode.Create);


            fs.Write(rawData, 0, rawData.Length);
            fs.Flush();
            fs.Close();
            fs = null;
            


            Encrypt ep = new Encrypt();
            rawData = ep.generateEncryptedPacket(rawData, false);

            return rawData;
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
