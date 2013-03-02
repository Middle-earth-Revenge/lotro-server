using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using Protocol;
using Protocol.Generic;
using Protocol.Session;
using Helper;
using Settings;
using Protocol.Server.Session;
using Account;

namespace Server
{
    class PacketHandler
    {
        MemoryStream memoryStreamInput = null;
        BEBinaryReader beBinaryReader = null;
        MemoryStream memoryStreamOutput = null;
        BEBinaryWriter beBinaryWriter = null;

        public void handleIncommingPacket(SocketObject socketObject)
        {
            memoryStreamInput = new MemoryStream(socketObject.Buffer, 0, socketObject.Length);
            beBinaryReader = new BEBinaryReader(memoryStreamInput, System.Text.Encoding.UTF8);

            memoryStreamOutput = new MemoryStream();
            beBinaryWriter = new BEBinaryWriter(memoryStreamOutput, System.Text.Encoding.UTF8);

            Payload payload = null;

            UInt16 sessionID = beBinaryReader.ReadUInt16BE();

            if (sessionID == 0x00) // Session setup payload
            {

                SetupHandler setupHandler = new SetupHandler();
                payload = setupHandler.process(socketObject, beBinaryReader);

                if (payload != null)
                {
                    Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, Server.UdpServer.Instance.packetNumberClient + "_in-" + payload.Data.GetType().Name, socketObject.Buffer, socketObject.Length, true);
                }
                else
                {
                    Debug.WriteLineIf(Config.Instance.Debug, "Failed to handle payload", DateTime.Now.ToString() + " ");
                }
                
            }
            else // Handle everything else
            {
                // look for existing client session
                Session clientSession = SessionHandler.Instance.getClientSession(sessionID, socketObject.EndPoint);

                if (clientSession == null || clientSession.SetupComplete == false) // do nothing, if session id is not present / valid or wrong ip address for id
                {
                    Debug.WriteLineIf(Config.Instance.Debug, "Client session id[" + sessionID + "] (ip: " + socketObject.EndPoint + ") was not found.", DateTime.Now.ToString() + " " );
                    return;
                }

                // Decrypt and process
                DecryptPacket dp = new DecryptPacket();
                beBinaryReader = dp.decryptClientPacket(beBinaryReader, sessionID); // afterwards readers base stream is now decrypted

                byte[] gg = beBinaryReader.ReadBytes(999);

                Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, Server.UdpServer.Instance.packetNumberClient + "_in-unknown", gg, gg.Length, true);
            }


            handleOutgoingPacket(socketObject, payload);
        }

        private void handleOutgoingPacket(SocketObject socketObject, Payload payload)
        {
            if (payload != null)
            {
                socketObject.Buffer = payload.Serialize(beBinaryWriter);
                socketObject.Length = (UInt16)socketObject.Buffer.Length;

                Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, Server.UdpServer.Instance.packetNumberClient + "_out-" + payload.Data.GetType().Name, socketObject.Buffer, socketObject.Length, true);

                UdpServer.Instance.addToSendQueue(socketObject);
            }
        }
    }
}
