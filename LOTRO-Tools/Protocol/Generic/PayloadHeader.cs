using System;
using System.IO;
using Helper;
using System.Diagnostics;
using System.Text;
using Settings;

namespace Protocol.Generic
{
    public class PayloadHeader
    {
        public PayloadHeader()
        {
            this.Length = 0x14; // Default header length
        }

        private const UInt32 defaultValue = 0xBADD70DD;

        public UInt16 SessionID { get; set; }

        public UInt16 DataLength { get; set; }

        public byte[] Action { get; set; }

        public UInt32 SequenceNumber { get; set; }

        public UInt32 HeaderChecksum { get; set; }

        public UInt32 ACKNR { get; set; }

        public UInt32 DataChecksum { get; set; }

        public UInt16 Length { get; set; }

        // Return: complete packet with added header (a complete server response)
        public byte[] Serialize(PayloadData protocolObject)
        {

            return new byte[] { 0x00 };
        }

        public PayloadHeader Deserialize(BEBinaryReader ber)
        {
            try
            {
                this.SessionID = ber.ReadUInt16BE();
                this.DataLength = ber.ReadUInt16BE();
                this.Action = ber.ReadBytes(4);
                this.SequenceNumber = ber.ReadUInt32BE();
                this.HeaderChecksum = ber.ReadUInt32BE();
                this.ACKNR = ber.ReadUInt32BE();
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

        // Return: array with header
        public byte[] Serialize(UInt32 XOrValue)
        {
            MemoryStream output = new MemoryStream();
            BEBinaryWriter bw = new BEBinaryWriter(output, Encoding.UTF8);

            // Create default header
            if(SequenceNumber == 0)
                bw.Write((short)0x00);

            bw.WriteUInt16BE(0xf1);//Settings.Config.Instance.ServerID); // ServerID
            bw.WriteUInt16BE(DataLength); // Data part length
            bw.Write(Action); // ActionA
            //bw.WriteUInt16BE(ActionB); // ActionB
            bw.WriteUInt32BE(SequenceNumber); // Sequence number
            bw.WriteUInt32BE(HeaderChecksum); // ChecksumA
            bw.WriteUInt32BE(ACKNR); // ChecksumB

            bw.Flush();

            byte[] tempHeader = output.ToArray();

            // Move this later to chksum function
            // Generate default header
            UInt32 tempHeaderChecksum = Helper.HelperMethods.Instance.getChecksumFromHeader(tempHeader);

            UInt32 defaultHeaderChecksum = Helper.HelperMethods.Instance.getDefaultHeadChecksum(tempHeader);

            UInt32 checksumFinal = DataChecksum ^ XOrValue;

            string xx = tempHeaderChecksum.ToString("x"); // 44188ef7

            UInt32 checksumToAdd = DataChecksum + tempHeaderChecksum; // Calculated data checksum minus tempheader-checksum

            string xy = checksumToAdd.ToString("x"); // 62b3518

            byte[] headerChecksum = null;

            if (XOrValue == 0)
            {
                headerChecksum = BitConverter.GetBytes(checksumToAdd); // valid checksum for header
            }
            else
            {
                headerChecksum = BitConverter.GetBytes(checksumFinal + defaultHeaderChecksum);
            }

            Array.Reverse(headerChecksum);

            Array.Copy(headerChecksum, 0, tempHeader, tempHeader.Length - 8, 4); // reverse and insert in header array          

            return tempHeader;
        }

        public byte[] Serialize(BEBinaryWriter beBinaryWriter)
        {
            beBinaryWriter.WriteUInt16BE(SessionID); // ServerID
            beBinaryWriter.WriteUInt16BE(DataLength); // Data part length
            beBinaryWriter.Write(Action); // ActionA
            beBinaryWriter.WriteUInt32BE(SequenceNumber); // Sequence number
            
            // Generate checksum value
            UInt32 checksumToInsert = Helper.HelperMethods.Instance.generateChecksumForHeader(SessionID, DataLength, Action, SequenceNumber, DataChecksum, ACKNR);

            beBinaryWriter.WriteUInt32BE(checksumToInsert); // Checksum
            beBinaryWriter.WriteUInt32BE(ACKNR); // ChecksumB
            beBinaryWriter.Flush();

            byte[] rawHeader = ((MemoryStream)beBinaryWriter.BaseStream).ToArray();

            beBinaryWriter.BaseStream.Position = 0;

            return rawHeader;
        }

        // Return: remaining bytes in packet (the data part) 
        public byte[] Deserialize(byte[] packet)
        {
            byte[] rawDataPart = null;

            MemoryStream ms = new MemoryStream(packet);
            BEBinaryReader ber = new BEBinaryReader(ms, Encoding.UTF8);
            
            try
            {
                this.SessionID = ber.ReadUInt16BE();

                if (SessionID == 0x00) // first or second packet from client (Auth-packets)
                {
                    SessionID = ber.ReadUInt16BE();

                    //if (head.ClientID == 0x00)
                    this.Length = 0x16;
                }
                else // packet with client session id is encrypted: decrypt byte[] and create new instance of reader
                {
                    DecryptPacket dp = new DecryptPacket();

                    //byte[] org = ber.ReadBytes(packet.Length - 2);

                    //byte[] decryptedPacket
                    packet = dp.generateDecryptedPacket(packet, true);

                    ms = new MemoryStream(packet);
                    ber = new BEBinaryReader(ms, Encoding.UTF8);

                    SessionID = ber.ReadUInt16BE();
                }

                this.DataLength = ber.ReadUInt16BE();

                this.Action = ber.ReadBytes(4);
                this.SequenceNumber = ber.ReadUInt32BE();
                this.HeaderChecksum = ber.ReadUInt32BE();
                this.ACKNR = ber.ReadUInt32BE();

                // Get Checksum for Header
                ber.BaseStream.Position = Length - 0x14;
                byte[] rawHeader = ber.ReadBytes(0x14);
                DataChecksum = Helper.HelperMethods.Instance.getChecksumFromHeader(rawHeader);
                

                rawDataPart = ber.ReadBytes(DataLength);

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

            return rawDataPart;
        }

        private UInt32 calcChecksum(byte[] header)
        {
            UInt32 checksum = 0x00;

            Array.Reverse(header); // hier noch beheben, ansonsten ist data anders herum!!!

            UInt32 defaultValue = BitConverter.ToUInt32(new byte[] { 0xdd, 0x70, 0xdd, 0xba }, 0);

            UInt32 firstValue = BitConverter.ToUInt32(new byte[] { 0x00, 0x00, 0x14, 0x00 }, 0);

            defaultValue += firstValue;

            checksum += BitConverter.ToUInt32(header, 1 * 4);

            defaultValue += BitConverter.ToUInt32(header, 0 * 4);

            defaultValue += BitConverter.ToUInt32(header, 2 * 4);

            defaultValue += BitConverter.ToUInt32(header, 3 * 4);

            defaultValue += BitConverter.ToUInt32(header, 4 * 4);

            return 0x00 - (defaultValue - checksum);

        }

    }
}
