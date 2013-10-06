using Helper;
using Protocol.Generic;
using Protocol.SessionSetup;
using SessionControl;
using Settings;
using System;
using System.Diagnostics;
using System.IO;

namespace Server
{
    public class SetupHandler
    {
        public static void process(SocketObject socketObject, BEBinaryReader beBinaryReader)
        {
            // Try to initialize the packet
            Payload payload = new PayloadSessionInit();
            payload.Deserialize(beBinaryReader);

            if (payload.Data is Protocol.Generic.WrongChecksum) // (first checksum check)
            {
                Debug.WriteLineIf(Config.Instance.Debug, "Client (ip: " + socketObject.EndPoint + ") WrongChecksum received", DateTime.Now.ToString() + " SetupHandler.process");
                return; // normally request packet because of failed checksum, but not at session setup
            }

            if (payload.Data is Protocol.SessionSetup.Synchronize) // (first client packet)
            {
                Debug.WriteLineIf(Config.Instance.Debug, "Client (ip: " + socketObject.EndPoint + ") Synchronize received", DateTime.Now.ToString() + " SetupHandler.process");

                Protocol.SessionSetup.Synchronize synchronize = (Protocol.SessionSetup.Synchronize)payload.Data;

                // Client is too old, update to a new version
                if (!synchronize.ClientVersion.Equals(Config.Instance.RequiredClientVersion))
                {
                    // Client version does not match expected server version (either to new or to old client)
                    Debug.WriteLineIf(Config.Instance.Debug, "Client (ip: " + socketObject.EndPoint + ") has old client version: " + synchronize.ClientVersion + ", expected: " + Config.Instance.RequiredClientVersion, DateTime.Now.ToString() + " SetupHandler.process");

                    OldClientVersion oldClientVersion = new OldClientVersion();
                    payload.Data = oldClientVersion;

                    payload.Header.SessionID = 0x00;
                }
                else
                {
                    // Client version matches expected server version, start a session
                    Session session = SessionControl.Handler.Instance.addSession(socketObject.EndPoint, synchronize.AccountName, synchronize.ClientVersion, synchronize.LocalTimeStarted, synchronize.GLSTicketDirect);

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

                        Debug.WriteLineIf(Config.Instance.Debug, "Client (ip: " + socketObject.EndPoint + ") now uses session id [" + session.ID + "]", DateTime.Now.ToString() + " SetupHandler.process");
                    }
                }

                PacketHandler.handleOutgoingPacket(socketObject, payload); // 2nd server packet send
                return;

            }

            if (payload.Data is Protocol.SessionSetup.Acknowledgment) // (second client packet)
            {
                Debug.WriteLineIf(Config.Instance.Debug, "Client (ip: " + socketObject.EndPoint + ") Acknowledgment received", DateTime.Now.ToString() + " SetupHandler.process");

                Protocol.SessionSetup.Acknowledgment acknowledgment = (Protocol.SessionSetup.Acknowledgment)payload.Data;

                Session session = SessionControl.Handler.Instance.getSession(payload.Header.SessionID, socketObject.EndPoint);

                if (session != null && acknowledgment.StartupSessionKey.Equals(session.StartupKey)) // && acknr is session + 1!!!
                {
                    session.SetupComplete = true;

                    // we start with session 00 00 00 02
                    session.SequenceNumberServer = 0x02;
                    session.SequenceNumberClient = 0x02;
                    session.ACKNRServer += 0x00010000;

                    Debug.WriteLineIf(Config.Instance.Debug, "Client session [" + payload.Header.SessionID + "] successful authenticated.", DateTime.Now.ToString() + " SetupHandler.process");

                    // this part has to be moved to client session with his own "handler" !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    // each client sesseion is responsible for his own packets

                    payload = new PayloadSession();
                    payload.Header = new PayloadHeader();


                    //2nd///////////////////////////////////////////////////////////////////////////////////////////////////////

                    payload.Data = new Protocol.Session.MultiDataObject();

                    DataObject dataObjectClientIP = new DataObject();

                    // create multi data packet
                    Protocol.Data.ClientIP clientIP = new Protocol.Data.ClientIP();
                    clientIP.ClientIPAddress = session.Endpoint.ToString().Split(':')[0];

                    ObjectHeader objectHeader = new Protocol.Header._06_81_49();

                    dataObjectClientIP.Header = objectHeader;
                    dataObjectClientIP.Data = clientIP;

                    payload.Data.DataObjectList.Add(dataObjectClientIP);

                    if (session.UserObject.FoundGLSTicketDirect == null) // if no glsticketdirect is submitted, kill client
                    {
                        DataObject dataObjectNoGLSTicketDirect = new DataObject();
                        Protocol.Data.NoGLSTicketDirect noGLSTicketDirect = new Protocol.Data.NoGLSTicketDirect();
                        noGLSTicketDirect.start = new byte[] { 0x00, 0x02, 0x00, 0xDB, 0x8C };
                        ObjectHeader objectHeader2 = new Protocol.Header._06_6D();

                        dataObjectNoGLSTicketDirect.Header = objectHeader2;
                        dataObjectNoGLSTicketDirect.Data = noGLSTicketDirect;

                        payload.Data.DataObjectList.Add(dataObjectNoGLSTicketDirect);

                        payload.Data.XORValue = session.getServerPayloadChecksumXOR(session.SequenceNumberServer);

                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Header.SequenceNumber = session.SequenceNumberServer;
                        payload.Header.ACKNR = session.ACKNRServer;

                        PacketHandler.handleOutgoingPacket(socketObject, payload); // 6th server packet send

                        session.SequenceNumberServer++;



                        Protocol.Server.Session.WrongGLSTicket endConnection = new Protocol.Server.Session.WrongGLSTicket();
                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Data = endConnection;
                        //payload.Header.SequenceNumber = 0x00000000;

                        PacketHandler.handleOutgoingPacket(socketObject, payload); // 6th server packet send

                        Protocol.Server.Session.TerminateServer endConnection2 = new Protocol.Server.Session.TerminateServer();
                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Data = endConnection2;
                        payload.Header.SequenceNumber = 0x00000000;

                        PacketHandler.handleOutgoingPacket(socketObject, payload); // 6th server packet send
                    }
                    else // ok glsticket there, no check if valid
                    {
                        DataObject someThing = new DataObject();
                        Protocol.Data._00_08 unknown = new Protocol.Data._00_08();

                        ObjectHeader objectHeader2 = new Protocol.Header._06_80_AA();

                        someThing.Header = objectHeader2;
                        someThing.Data = unknown;

                        payload.Data.DataObjectList.Add(someThing);
                    }


                    payload.Data.XORValue = session.getServerPayloadChecksumXOR(session.SequenceNumberServer);

                    payload.Header.SessionID = Config.Instance.ServerId;
                    payload.Header.SequenceNumber = session.SequenceNumberServer;
                    payload.Header.ACKNR = session.ACKNRServer;

                    PacketHandler.handleOutgoingPacket(socketObject, payload); // 2nd server packet send

                    session.SequenceNumberServer++;


                    if (session.UserObject.GLSTicketDirect != null && session.UserObject.GLSTicketDirect.Equals(session.UserObject.FoundGLSTicketDirect))
                    {

                        //3nd/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                        payload.Data = new Protocol.Session.MultiDataObject();

                        DataObject someThing1 = new DataObject();
                        Protocol.Data._00_01_D6_80 unknown1 = new Protocol.Data._00_01_D6_80();

                        someThing1.Header = objectHeader;
                        someThing1.Data = unknown1;

                        payload.Data.DataObjectList.Add(someThing1);

                        someThing1 = new DataObject();
                        Protocol.Data.AccountName accountName = new Protocol.Data.AccountName();
                        accountName.Account = session.UserObject.AccountName;

                        someThing1.Header = objectHeader;
                        someThing1.Data = accountName;

                        payload.Data.DataObjectList.Add(someThing1);

                        someThing1 = new DataObject();
                        Protocol.Data._00_78_66 unknown2 = new Protocol.Data._00_78_66();

                        someThing1.Header = objectHeader;
                        someThing1.Data = unknown2;

                        payload.Data.DataObjectList.Add(someThing1);

                        someThing1 = new DataObject();
                        Protocol.Data._00_06_6B unknown3 = new Protocol.Data._00_06_6B();

                        someThing1.Header = objectHeader;
                        someThing1.Data = unknown3;

                        payload.Data.DataObjectList.Add(someThing1);

                        payload.Data.XORValue = session.getServerPayloadChecksumXOR(session.SequenceNumberServer);

                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Header.SequenceNumber = session.SequenceNumberServer;
                        payload.Header.ACKNR = session.ACKNRServer;


                        PacketHandler.handleOutgoingPacket(socketObject, payload); // 3nd server packet send

                        session.SequenceNumberServer++;

                        //4th///////////////////////////////////////////////////////////////////////////////////////////////////////

                        payload.Data = new Protocol.Session.MultiDataObject();
                        ObjectHeader objectHeader3 = new Protocol.Header._06_80_AA();

                        DataObject someThing2 = new DataObject();
                        Protocol.Data._00_08 unknown4 = new Protocol.Data._00_08();
                        unknown4.data = new byte[] { 0x01, 0x8D, 0xE5, 0x02 };

                        someThing2.Header = objectHeader3;
                        someThing2.Data = unknown4;

                        payload.Data.DataObjectList.Add(someThing2);

                        someThing2 = new DataObject();
                        Protocol.Data._00_08 unknown5 = new Protocol.Data._00_08();
                        unknown5.data = new byte[] { 0x14, 0x2D, 0x99, 0x01 };

                        someThing2.Header = objectHeader3;
                        someThing2.Data = unknown5;

                        payload.Data.DataObjectList.Add(someThing2);

                        payload.Data.XORValue = session.getServerPayloadChecksumXOR(session.SequenceNumberServer);

                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Header.SequenceNumber = session.SequenceNumberServer;
                        payload.Header.ACKNR = session.ACKNRServer;

                        PacketHandler.handleOutgoingPacket(socketObject, payload); // 4th server packet send

                        session.SequenceNumberServer++;

                        //5th///////////////////////////////////////////////////////////////////////////////////////////////////////

                        payload.Data = new Protocol.Session.MultiDataObject();
                        ObjectHeader objectHeader4 = new Protocol.Header._06_43();

                        DataObject someThing3 = new DataObject();
                        Protocol.Data.ServerName serverName = new Protocol.Data.ServerName();
                        serverName.Server = session.UserObject.ServerName;

                        someThing3.Header = objectHeader4;
                        someThing3.Data = serverName;

                        payload.Data.DataObjectList.Add(someThing3);

                        someThing3 = new DataObject();
                        Protocol.Data._00_01_00_00 unknown6 = new Protocol.Data._00_01_00_00();

                        objectHeader4 = new Protocol.Header._06_80_DD();

                        someThing3.Header = objectHeader4;
                        someThing3.Data = unknown6;

                        payload.Data.DataObjectList.Add(someThing3);

                        payload.Data.XORValue = session.getServerPayloadChecksumXOR(session.SequenceNumberServer);

                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Header.SequenceNumber = session.SequenceNumberServer;
                        payload.Header.ACKNR = session.ACKNRServer;

                        PacketHandler.handleOutgoingPacket(socketObject, payload); // 5th server packet send

                        payload.Header.ACKNR += 0x00010000;

                        session.SequenceNumberServer++;



                        // client could crash if he misses this packet, need a better missed packet handling, so that it could be resend very fast
                        // til now only a little break helps



                        //6th//////////////////////////////////////////////////////////////////////////////////////////////////

                        payload.Data = new Protocol.Session.MultiDataObject();
                        ObjectHeader objectHeader5 = new Protocol.Header._06_81_49();

                        DataObject someThing5 = new DataObject();
                        Protocol.Data._00_01_D7 unknown7 = new Protocol.Data._00_01_D7();

                        someThing5.Header = objectHeader5;
                        someThing5.Data = unknown7;

                        payload.Data.DataObjectList.Add(someThing5);

                        someThing5 = new DataObject();
                        Protocol.Data._00 unknown8 = new Protocol.Data._00();

                        objectHeader5 = new Protocol.Header._06_80_F5();

                        someThing5.Header = objectHeader5;
                        someThing5.Data = unknown8;

                        payload.Data.DataObjectList.Add(someThing5);

                        payload.Data.XORValue = session.getServerPayloadChecksumXOR(session.SequenceNumberServer);

                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Header.SequenceNumber = session.SequenceNumberServer;
                        payload.Header.ACKNR = session.ACKNRServer;

                        PacketHandler.handleOutgoingPacket(socketObject, payload); // 6th server packet send

                        session.SequenceNumberServer++;



                        //7th/////Character Data///////////////////////////////////////////////////////////////////////////////////

                        payload.Data = new Protocol.Session.MultiDataObject();
                        ObjectHeader oh1 = new Protocol.Header._06_80_AA();

                        DataObject st1 = new DataObject();
                        Protocol.Data._00_08 u1 = new Protocol.Data._00_08();
                        u1.data = new byte[] { 0x24, 0x82, 0x9F, 0x01 };

                        st1.Header = oh1;
                        st1.Data = u1;

                        payload.Data.DataObjectList.Add(st1);

                        oh1 = new Protocol.Header._06_54();

                        st1 = new DataObject();
                        Protocol.Data.EncAccountName u2 = new Protocol.Data.EncAccountName();
                        u2.EncAccount = session.UserObject.AccountName;

                        st1.Header = oh1;
                        st1.Data = u2;

                        payload.Data.DataObjectList.Add(st1);




                        // chara data part

                        oh1 = new Protocol.Header._06_6C_80();

                        st1 = new DataObject();
                        Protocol.Data.CharaData cd = new Protocol.Data.CharaData();
                        cd.PlainAccountName = session.UserObject.AccountName;
                        cd.ClientIP = session.UserObject.IPAddress;
                        cd.EncAccountName = session.UserObject.AccountName;

                        st1.Header = oh1;
                        st1.Data = cd;

                        payload.Data.DataObjectList.Add(st1);

                        // chara data part


                        payload.Data.XORValue = session.getServerPayloadChecksumXOR(session.SequenceNumberServer);

                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Header.SequenceNumber = session.SequenceNumberServer;
                        payload.Header.ACKNR = session.ACKNRServer;

                        PacketHandler.handleOutgoingPacket(socketObject, payload); // 6th server packet send

                        session.SequenceNumberServer++;

                    }
                    else
                    {
                        payload.Data = new Protocol.Session.MultiDataObject();

                        DataObject dataObjectNoGLSTicketDirect = new DataObject();
                        Protocol.Data.NoGLSTicketDirect noGLSTicketDirect = new Protocol.Data.NoGLSTicketDirect();
                        noGLSTicketDirect.start = new byte[] {0x00,0x03,0x00,0xDB,0x8C };
                        ObjectHeader objectHeader2 = new Protocol.Header._06_6D();

                        dataObjectNoGLSTicketDirect.Header = objectHeader2;
                        dataObjectNoGLSTicketDirect.Data = noGLSTicketDirect;

                        payload.Data.DataObjectList.Add(dataObjectNoGLSTicketDirect);

                        payload.Data.XORValue = session.getServerPayloadChecksumXOR(session.SequenceNumberServer);

                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Header.SequenceNumber = session.SequenceNumberServer;
                        payload.Header.ACKNR = session.ACKNRServer;

                        PacketHandler.handleOutgoingPacket(socketObject, payload); // 6th server packet send

                        session.SequenceNumberServer++;



                        Protocol.Server.Session.WrongGLSTicket endConnection = new Protocol.Server.Session.WrongGLSTicket();
                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Data = endConnection;
                        //payload.Header.SequenceNumber = 0x00000000;

                        PacketHandler.handleOutgoingPacket(socketObject, payload); // 6th server packet send

                        Protocol.Server.Session.TerminateServer endConnection2 = new Protocol.Server.Session.TerminateServer();
                        payload.Header.SessionID = Config.Instance.ServerId;
                        payload.Data = endConnection2;
                        payload.Header.SequenceNumber = 0x00000000;

                        PacketHandler.handleOutgoingPacket(socketObject, payload); // 6th server packet send
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////////

                }

                return;
            }

            // are there more cases?

            Debug.WriteLineIf(Config.Instance.Debug, "Invalid packet received", DateTime.Now.ToString() + " SetupHandler.process");
        }
    }
}
