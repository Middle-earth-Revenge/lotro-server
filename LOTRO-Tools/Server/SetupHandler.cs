using System;
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

                if (!synchronize.ClientVersion.Equals(Config.Instance.RequiredClientVersion))
                {
                    // Client version does not match expected server version (either to new or to old client)
                    Debug.WriteLineIf(Config.Instance.Debug, "Client (ip: " + socketObject.EndPoint + ") has old client version: " + synchronize.ClientVersion + ", expected: " + Config.Instance.RequiredClientVersion, DateTime.Now.ToString() + " ");

                    OldClientVersion oldClientVersion = new OldClientVersion();
                    payload.Data = oldClientVersion;

                    payload.Header.SessionID = 0x00;                  
                }
                else
                {
                    // Client version matches expected server version, start a session
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

            }
            else if (payload.Data is Protocol.SessionSetup.Acknowledgment) // (second client packet)
            {
                Debug.WriteLineIf(Config.Instance.Debug, "Client (ip: " + socketObject.EndPoint + ") Acknowledgment received", DateTime.Now.ToString());

                Protocol.SessionSetup.Acknowledgment acknowledgment = (Protocol.SessionSetup.Acknowledgment)payload.Data;

                Session clientSession = SessionHandler.Instance.getClientSession(payload.Header.SessionID, socketObject.EndPoint);

                if (clientSession == null)
                {
                    // We don't know about this client sesion
                    Debug.WriteLineIf(Config.Instance.Debug, "Client session [" + payload.Header.SessionID + "] not found.", DateTime.Now.ToString() + " ");
                }
                else if (acknowledgment.StartupSessionKey.Equals(clientSession.StartupKey)) // && acknr is session + 1!!!
                {
                    clientSession.SetupComplete = true;

                    Debug.WriteLineIf(Config.Instance.Debug, "Client session [" + payload.Header.SessionID + "] successful authenticated.", DateTime.Now.ToString() + " ");
                }
                else
                {
                    // Startup key does not match
                    Debug.WriteLineIf(Config.Instance.Debug, "Client session [" + payload.Header.SessionID + "] did not match startup session key.", DateTime.Now.ToString() + " ");
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
            else
            {
                Debug.WriteLineIf(Config.Instance.Debug, "Client (ip: " + socketObject.EndPoint + ") sent unknown payload", DateTime.Now.ToString());
            }

            // are there more cases?

            return payload;
        }
    }
}
