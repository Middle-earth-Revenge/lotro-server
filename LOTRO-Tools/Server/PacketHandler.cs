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
using AccountControl;
using SessionControl;

namespace Server
{
    class PacketHandler
    {
        public void handleIncommingPacket(SocketObject socketObject)
        {
            // First of all: create the reader for the incoming packet
            MemoryStream memoryStreamInput = new MemoryStream(socketObject.Buffer, 0, socketObject.Length);
            BEBinaryReader beBinaryReader = new BEBinaryReader(memoryStreamInput, System.Text.Encoding.UTF8);

            Payload payload = null;

            UInt16 sessionID = beBinaryReader.ReadUInt16BE();

            if (sessionID == 0x00) // Session setup payload
            {
                //Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, "received-" + Server.UdpServer.Instance.packetNumberClient, socketObject.Buffer, socketObject.Length, true);

                SetupHandler setupHandler = new SetupHandler();
                payload = setupHandler.process(socketObject, beBinaryReader);

                //if(payload != null)
                //    Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, payload.Data.GetType().Name + Server.UdpServer.Instance.packetNumberClient, socketObject.Buffer, socketObject.Length, true);               
            }
            else // Handle everything else
            {
                // look for existing client session
                Session session = SessionControl.Handler.Instance.getSession(sessionID, socketObject.EndPoint);

                if (session == null || session.SetupComplete == false) // do nothing, if session id is not present / valid or wrong ip address for id
                {
                    Debug.WriteLineIf(Config.Instance.Debug, "Client session id[" + sessionID + "] (ip: " + socketObject.EndPoint + ") was not found.", DateTime.Now.ToString() + " " );
                    return;
                }

                // Decrypt and process
                DecryptPacket dp = new DecryptPacket();
                beBinaryReader = dp.decryptClientPacket(beBinaryReader, sessionID); // afterwards readers base stream is now decrypted

                byte[] gg = beBinaryReader.ReadBytes(999);

                beBinaryReader.BaseStream.Position = 0;

                if (gg[7] == 0x06)
                {
                    Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, "data-" + Server.UdpServer.Instance.packetNumberClient, gg, gg.Length, true);

                    // not generic enough, get's not every packet
                    if (gg[0x14] == 0x01 && gg[0x15] == 0x00 && gg[0x16] == 0x01 && gg[0x17] == 0x04 && session.UserObject.AccountName.Equals("admin"))
                    {
                        payload = new PayloadSession();
                        payload.Header = new PayloadHeader();

                        payload.Data = new Protocol.Session.MultiDataObject();
                        ObjectHeader objectHeader3 = new Protocol.Header._06_80_93();
                        // ObjectHeader objectHeader3 = new Protocol.Header._06_79();

                        DataObject someThing2 = new DataObject();
                        Protocol.Data.RecCharaData premadeChara = new Protocol.Data.RecCharaData();
                        // Protocol.Data.BadCharaName unknown4 = new Protocol.Data.BadCharaName();
                        // unknown4.start = new byte[] { 0x00, 0x18, 0x00, 0xDB, 0x8C, 0x2b, 0x00, 0x00, 0x00 }; // change 2b to 33, 32, 10, 41, 3b for different cases
                        premadeChara.characterName = session.UserObject.AccountName;



                        

                        someThing2.Header = objectHeader3;
                        someThing2.Data = premadeChara;

                        payload.Data.DataObjectList.Add(someThing2);

                        payload.Data.XORValue = session.getServerPayloadChecksumXOR(session.SequenceNumberServer);

                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Header.SequenceNumber = session.SequenceNumberServer;
                        payload.Header.ACKNR = session.ACKNRServer;

                        handleOutgoingPacket(socketObject, payload); // 4th server packet send

                        session.SequenceNumberServer++;
                    }


                }

                payload = new PayloadSession();
                payload.Deserialize(beBinaryReader);

                if (payload.Data != null)
                {

                    if (payload.Data is Protocol.Server.Session.ConfirmSequence) // (first checksum check)
                    {
                        Protocol.Server.Session.ConfirmSequence confirmSequence = (Protocol.Server.Session.ConfirmSequence)payload.Data;
                        UInt32 lastReceivedSequenceNr = confirmSequence.sequenceNr;

                        confirmSequence.sequenceNr = payload.Header.SequenceNumber;


                        Protocol.Server.Session.ConfirmSequence answer = new Protocol.Server.Session.ConfirmSequence();

                        answer.sequenceNr = payload.Header.SequenceNumber-1;

                        payload.Data = answer;

                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Header.ACKNR = (session.ACKNRServer + 0x00040000);
                        payload.Header.SequenceNumber = lastReceivedSequenceNr; 

                        handleOutgoingPacket(socketObject, payload);

                        session.ACKNRServer = payload.Header.ACKNR;

                    }
                }
            
            }          

            
        }

        private void handleOutgoingPacket(SocketObject socketObject, Payload payload)
        {
            if (payload != null)
            {
                MemoryStream memoryStreamOutput = new MemoryStream();
                BEBinaryWriter beBinaryWriter = new BEBinaryWriter(memoryStreamOutput, System.Text.Encoding.UTF8);

                socketObject.Buffer = payload.Serialize(beBinaryWriter);
                socketObject.Length = (UInt16)socketObject.Buffer.Length;

                //Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, "send-" + Server.UdpServer.Instance.packetNumberClient, socketObject.Buffer, socketObject.Length, true);

                UdpServer.Instance.addToSendQueue(socketObject);
            }
        }
    }
}
