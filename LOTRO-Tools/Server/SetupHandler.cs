﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Protocol.Generic;
using System.Diagnostics;
using Settings;
using Helper;
using Protocol.SessionSetup;
using Account;
using System.IO;

namespace Server
{
    public class SetupHandler
    {
        public Payload process(SocketObject socketObject, BEBinaryReader beBinaryReader)
        {
            Payload payload = new PayloadSessionInit();
            payload.Deserialize(beBinaryReader);

            if (payload.Data is Protocol.SessionSetup.Synchronize) // (first client packet)
            {
                Debug.WriteLineIf(Config.Instance.Debug, "Client (ip: " + socketObject.EndPoint + ") Synchronize received", DateTime.Now.ToString() + " ");

                Protocol.SessionSetup.Synchronize synchronize = (Protocol.SessionSetup.Synchronize)payload.Data;

                // Client is too old, update to a new version
                if (!synchronize.ClientVersion.Equals(Config.Instance.RequiredClientVersion))
                {
                    Debug.WriteLineIf(Config.Instance.Debug, "Client (ip: " + socketObject.EndPoint + ") has old client version.", DateTime.Now.ToString() + " ");

                    OldClientVersion oldClientVersion = new OldClientVersion();
                    payload.Data = oldClientVersion;

                    payload.Header.SessionID = 0x00;                  
                }
                else
                {
                    Session session = SessionHandler.Instance.addClientSession(socketObject.EndPoint, synchronize.AccountName, synchronize.ClientVersion, synchronize.LocalTimeStarted);

                    if (session != null)
                    {

                        SYNACK synACK = new SYNACK();

                        synACK.SessionID = session.ID;
                        synACK.StartupSessionKey = session.StartupKey;
                        synACK.ChecksumServer = session.InitialChecksumServer;
                        synACK.ChecksumClient = session.InitialChecksumClient;

                        payload.Data = synACK;

                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Header.ACKNR = session.ACKNRServer;

                        Debug.WriteLineIf(Config.Instance.Debug, "Client (ip: " + socketObject.EndPoint + ") now uses session id [" + session.ID + "]", DateTime.Now.ToString() + " ");
                    }
                    // else server full packet
                }

            } // Synchronize

            if (payload.Data is Protocol.SessionSetup.Acknowledgment) // (second client packet)
            {
                Debug.WriteLineIf(Config.Instance.Debug, "Client (ip: " + socketObject.EndPoint + ") Acknowledgment received", DateTime.Now.ToString());

                Protocol.SessionSetup.Acknowledgment acknowledgment = (Protocol.SessionSetup.Acknowledgment)payload.Data;

                Session clientSession = SessionHandler.Instance.getClientSession(payload.Header.SessionID, socketObject.EndPoint);

                if (clientSession != null && acknowledgment.StartupSessionKey.Equals(clientSession.StartupKey)) // && acknr is session + 1!!!
                {
                    clientSession.SetupComplete = true;

                    Debug.WriteLineIf(Config.Instance.Debug, "Client session [" + payload.Header.SessionID + "] successful authenticated.", DateTime.Now.ToString() + " ");
                }


                // not what we want... no binary data reading and sending...!!!
                /*for (int i = 3; i < 7; i++)
                {

                    byte[] bytes = File.ReadAllBytes(@"Data\\" + i + "_server-9003");

                    Helper.Encrypt enc = new Encrypt();

                    bytes = enc.generateEncryptedPacket(bytes, false);

                    socketObject.Buffer = bytes;
                    socketObject.Length = (UInt16)bytes.Length;

                    Server.UdpServer.Instance.addToSendQueue(socketObject);

                }*/

                payload = null;
            }

            // are there more cases?

            return payload;
        }
    }
}