using Helper;
using Protocol.Generic;
using System;
using System.IO;
using System.Text;

namespace Server
{
    class PacketHandler
    {
        private static int dumpCounter = 0;

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

        public void handleIncommingPacket(SocketObject socketObject)
        {
            // First of all: create the reader for the incoming packet
            MemoryStream memoryStreamInput = new MemoryStream(socketObject.Buffer, 0, socketObject.Length);
            BEBinaryReader beBinaryReader = new BEBinaryReader(memoryStreamInput, System.Text.Encoding.UTF8);

            UInt16 sessionID = beBinaryReader.ReadUInt16BE();
            beBinaryReader.BaseStream.Position = 0;

            if (sessionID == 0x0000) // Session setup payload
            {
                // Skip two bytes
                beBinaryReader.ReadUInt16BE();

                long tmp = beBinaryReader.BaseStream.Position;
                byte[] data = beBinaryReader.ReadBytes(999);
                beBinaryReader.BaseStream.Position = tmp;

                string postfix = BitConverter.ToString(data, 4, 4).Replace("-", "");

                Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, String.Format("{0,4:0000}", dumpCounter++) + "_client-" + postfix, new System.Text.UTF8Encoding().GetBytes(ToString(data)), new System.Text.UTF8Encoding().GetBytes(ToString(data)).Length, true);
                
                SetupHandler.process(socketObject, beBinaryReader);
            }
            else // Handle everything else
            {
                // Decrypt the incoming packet
                // afterwards the beBinaryReader's base stream is decrypted
                DecryptPacket dp = new DecryptPacket();
                beBinaryReader = dp.decryptClientPacket(beBinaryReader, sessionID);

                long tmp = beBinaryReader.BaseStream.Position;
                byte[] data = beBinaryReader.ReadBytes(999);
                beBinaryReader.BaseStream.Position = tmp;

                string postfix = BitConverter.ToString(data, 4, 4).Replace("-", "");

                Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, String.Format("{0,4:0000}", dumpCounter++) + "_client-" + postfix, new System.Text.UTF8Encoding().GetBytes(ToString(data)), new System.Text.UTF8Encoding().GetBytes(ToString(data)).Length, true);

                SessionHandler.process(sessionID, socketObject, beBinaryReader);
            }          

            
        }

        public static void handleOutgoingPacket(SocketObject socketObject, Payload payload)
        {
            if (payload != null)
            {
                MemoryStream memoryStreamOutput = new MemoryStream();
                BEBinaryWriter beBinaryWriter = new BEBinaryWriter(memoryStreamOutput, System.Text.Encoding.UTF8);

                socketObject.Buffer = payload.Serialize(beBinaryWriter, dumpCounter++);
                socketObject.Length = (UInt16)socketObject.Buffer.Length;

                UdpServer.Instance.addToSendQueue(socketObject);
            }
        }

    }
}
