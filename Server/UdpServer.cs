﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using Protocol.Generic;
using Protocol.Session;
using Protocol;
using System.Collections;
using Settings;
using System.Threading;
using AccountControl;
using Helper;
using Protocol.Server.Session;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Protocol.SessionSetup;

namespace Server
{
    public sealed class UdpServer
    {
        // Everything singleton related
        private static volatile UdpServer instance;
        private static readonly object threadLock = new Object();
        public static UdpServer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (threadLock)
                    {
                        if (instance == null)
                        {
                            instance = new UdpServer();
                        }
                    }
                }
                return instance;
            }
        }

        private Socket serverSocket;

        private BlockingCollection<SocketObject> receiveQueue = new BlockingCollection<SocketObject>();
        private BlockingCollection<SocketObject> sendQueue = new BlockingCollection<SocketObject>();

        private bool isRunning = false;

        public UInt32 packetNumberClient = 0;


        public bool startServer()
        {
            isRunning = openReceiverSocket(Config.Instance.ServerPort);

            Thread sendWorker = new Thread(startSendQueue);
            sendWorker.IsBackground = true;
            sendWorker.Priority = ThreadPriority.Normal;
            sendWorker.Name = "sendWorker";
            sendWorker.Start();

            Thread receiveWorker = new Thread(startReceiveQueue);
            receiveWorker.IsBackground = true;
            receiveWorker.Priority = ThreadPriority.Highest;
            receiveWorker.Name = "receiveWorker";
            receiveWorker.Start();

            listenForData();
            return isRunning;
        }

        public void stopServer()
        {
            isRunning = false;

            if (serverSocket != null)
            {
                serverSocket.Shutdown(SocketShutdown.Both);
                serverSocket.Close();
                serverSocket.Dispose();
                serverSocket = null;
            }
        }

        private void startReceiveQueue()
        {
            while (isRunning)
            {
                SocketObject socketObject = receiveQueue.Take();
                ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveData), socketObject);
            }

        }

        private void startSendQueue()
        {
            while (isRunning)
            {
                SocketObject socketObject = sendQueue.Take();
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendData), socketObject);
            }

        }

        private bool openReceiverSocket(UInt16 port)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                //serverSocket.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.NoDelay, false);
                serverSocket.Poll(4000, SelectMode.SelectRead);

                IPEndPoint receiverEndPoint = new IPEndPoint(IPAddress.Any, port);
                serverSocket.Bind(receiverEndPoint);

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLineIf(Config.Instance.Debug, e.Message, DateTime.Now.ToString() + " " + this.GetType().Name + ".openReceiverSocket");
            }

            return false;
        }

        private void listenForData()
        {
            SocketObject socketPacket = new SocketObject();

            try
            {
                serverSocket.BeginReceiveFrom(socketPacket.Buffer, 0, socketPacket.Buffer.Length, SocketFlags.None, ref socketPacket.EndPoint, new AsyncCallback(OnReceiveData), socketPacket);
            }
            catch (Exception e)
            {
                Debug.WriteLineIf(Config.Instance.Debug, "BeginReceiveFrom error: " + e.Message, DateTime.Now.ToString() + " " + this.GetType().Name + ".listenForData");
            }

        }

        private void OnReceiveData(IAsyncResult result)
        {
            SocketObject socketPacket = null;

            try
            {
                socketPacket = (SocketObject)result.AsyncState;
                socketPacket.Length = (UInt16)serverSocket.EndReceiveFrom(result, ref socketPacket.EndPoint);

                //Debug.WriteLineIf(Config.Instance.Debug, string.Format("Received {0} bytes from client (ip: " + socketPacket.EndPoint + ")", socketPacket.Length), DateTime.Now.ToString() + " " + this.GetType().Name + ".OnReceiveData");

                if (socketPacket.Length >= 13)  // Add received packet to receiver queue
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(addToReceiveQueue), socketPacket);                
                }
                else // If size too small don't process
                {
                    throw new Exception("Error. Packet from connection (ip: " + socketPacket.EndPoint + ") is too short.");
                }

            }
            catch (SocketException socketException)
            {
                Debug.WriteLineIf(Config.Instance.Debug, socketException.Message, DateTime.Now.ToString() + " " + this.GetType().Name + ".OnReceiveData");
                Debug.Flush();
            }
            catch (Exception e)
            {
                Debug.WriteLineIf(Config.Instance.Debug, e.Message, DateTime.Now.ToString() + " " + this.GetType().Name + ".OnReceiveData");
                Debug.Flush();

                if (socketPacket != null)
                {
                    byte[] rawPacket = new byte[socketPacket.Length];
                    Array.Copy(socketPacket.Buffer, rawPacket, socketPacket.Length);

                    if (socketPacket.Buffer.Length >= 2)
                    {

                        if (socketPacket.Buffer[0] == 0x00 & socketPacket.Buffer[1] == 0x00)
                        {
                            Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, "exception-packet-" + packetNumberClient, rawPacket, rawPacket.Length, true);
                        }
                        else
                        {
                            Helper.Decrypt dp = new Helper.Decrypt();

                            byte[] decrypted = dp.generateDecryptedPacket(rawPacket, true);

                            Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, "exception-packet-" + packetNumberClient, decrypted, decrypted.Length, true);
                        }
                    }


                }
            }

            packetNumberClient++;
            listenForData();
        }

        private void OnSendData(IAsyncResult result)
        {
            try
            {
                SocketObject packet = (SocketObject)result.AsyncState;

                int bytesSent = serverSocket.EndSendTo(result);
                Debug.WriteLineIf(Config.Instance.Debug, string.Format("Sent {0} bytes to client (ip: " + packet.EndPoint + ")", packet.Length), DateTime.Now.ToString() + " " + this.GetType().Name + ".OnSendData");

            }
            catch (Exception e)
            {
                Debug.WriteLineIf(Config.Instance.Debug, e.ToString(), DateTime.Now.ToString() + " " + this.GetType().Name + ".OnSendData");
            }
        }

        private void SendData(object passedObject)
        {
            try
            {
                SocketObject socketObject = (SocketObject)passedObject;
                serverSocket.BeginSendTo(socketObject.Buffer, 0, socketObject.Length, SocketFlags.None, socketObject.EndPoint, new AsyncCallback(OnSendData), socketObject);
            }
            catch (InvalidCastException ice)
            {
                Debug.WriteLineIf(Config.Instance.Debug, ice.ToString(), DateTime.Now.ToString() + " " + this.GetType().Name + ".SendPacket");
            }
        }

        private void ReceiveData(object passedObject)
        {
            try
            {
                SocketObject socketObject = (SocketObject)passedObject;
                PacketHandler packetHandler = new PacketHandler();
                packetHandler.handleIncommingPacket(socketObject);
            }
            catch (InvalidCastException ice)
            {
                Debug.WriteLineIf(Config.Instance.Debug, ice.ToString(), DateTime.Now.ToString() + " " + this.GetType().Name + ".SendPacket");
            }
        }

        // Received packets will be added to the receiver queue
        private void addToReceiveQueue(object passedObject)
        {
            try
            {
                SocketObject socketObject = (SocketObject)passedObject;

                if (socketObject != null)
                {
                    receiveQueue.Add(socketObject);
                }
            }
            catch (InvalidCastException ice)
            {
                Debug.WriteLineIf(Config.Instance.Debug, ice.ToString(), DateTime.Now.ToString() + " " + this.GetType().Name + ".addToReceiverQueue");
            }
        }

        // Adds SocketObjects to send queue
        public void addToSendQueue(SocketObject socketObject)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(addToSQueue), socketObject);
        }

        private void addToSQueue(object passedObject)
        {
            try
            {
                SocketObject socketObject = (SocketObject)passedObject;

                if (socketObject != null && socketObject.Length > 0)
                {
                    sendQueue.Add(socketObject);
                }
                else
                {
                    if (socketObject != null)
                    {
                        Debug.WriteLineIf(Config.Instance.Debug, "Failed to add object: " + socketObject.ToString(), DateTime.Now.ToString() + " " + this.GetType().Name + ".addToQueue");
                        Debug.WriteLineIf(Config.Instance.Debug, "Object length: " + socketObject.Length, DateTime.Now.ToString() + " " + this.GetType().Name + ".addToQueue");
                    }
                    else
                    {
                        Debug.WriteLineIf(Config.Instance.Debug, "Failed to add object, because it's null", DateTime.Now.ToString() + " " + this.GetType().Name + ".addToQueue");
                    }
                }
            }
            catch (InvalidCastException ice)
            {
                Debug.WriteLineIf(Config.Instance.Debug, ice.ToString(), DateTime.Now.ToString() + " " + this.GetType().Name + ".addToQueue");
            }

        }


    }
}
