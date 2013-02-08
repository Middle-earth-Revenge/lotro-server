using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using LOTRO;

/* Start the lotro client with: lotroclient.exe -a somefakeaccount -h 127.0.0.1:9000 -language de
 * or language en ;-)
 */

namespace LOTROServer
{
    class Program
    {
        private int numberClientPacket = 2;
        private UdpClient client = null;
        //private TcpListener tcp = null;
        private IPEndPoint anyIP = null;
        private bool inSession = false;

        private readonly string packetName = "server1packets\\packet"; // please change to your needs
        private readonly string packetNameOutputOrg = "clientlog_org\\packet"; // please change to your needs
        private readonly string packetNameOutputDec = "clientlog_dec\\packet"; // please change to your needs

        private byte[] firstClientPacketBytes = { 0x0, 0x0, 0x0, 0x0 }; // indicator for first client packet
        private byte[] secondClientPacketBytes = { 0x0, 0x0 }; // indicator for second client packet
        private byte[] clientSessionStartBytes = new byte[2]; // the beginning of every client packet

        private Decrypt decryptClientPacket = new Decrypt(); // automatically decrypts client packets to the given folder       

        private byte[] handleClientAuth(BinaryReader br, UdpClient udp, IPEndPoint iep)
        {
            byte[] answerData = null;

            ushort positionString = br.ReadUInt16(); // Position von Client String im Byte Array, d.h. bis zum verschlüsselten Account-Namen, in diesem Fall 239 Bytes
            ushort i2 = br.ReadUInt16(); // 00 00
            ushort a = br.ReadUInt16(); // 01 00 // 1
            ushort i3 = br.ReadUInt16(); // 00 00
            ushort i4 = br.ReadUInt16(); // 00 00
            uint dueTime = br.ReadUInt32(); // c7 9c 22 e7 //1701909219 // Token aus Zeit? Welche Zeit?
            ushort i5 = br.ReadUInt16(); // 00 00
            ushort i6 = br.ReadUInt16(); // 00 00
            string clientVersion = br.ReadString(); // Client Version, ändert sich nur beim Update 061004_netver:7088; didver:926CD8E3-2984-4CA9-9C6B-6DF2C8EB6BC3
            uint lenghtRest = br.ReadUInt32(); // AB 00 00 00 // wieviele Daten kommen noch bis Paket zuende? 171
            ushort b = br.ReadUInt16(); // 01 00
            ushort i8 = br.ReadUInt16(); // 00 00
            ushort c = br.ReadUInt16(); // 01 00
            ushort i9 = br.ReadUInt16(); // 00 00
            uint dateTimeSpanClientStarted = br.ReadUInt32(); // 29 2d e5 4e

            // Session SimpleBlob
            //uint lenghtBlob = br.ReadUInt32(); // 8c 00 00 00 (140) Länge von Blob
            //byte[] simpleBlob = br.ReadBytes((int)lenghtBlob); // encrypted session key inside simple blob

            uint lenghtEncrypted = br.ReadUInt32(); // 51 01 00 00 // Account verschlüsselt
            byte[] encrypted = br.ReadBytes((int)lenghtEncrypted); // 

            TimeSpan span = TimeSpan.FromSeconds(dateTimeSpanClientStarted);
            DateTime clientStarted = new DateTime(1970, 1, 1).Add(span).ToLocalTime();

            System.Diagnostics.Debug.WriteLine("positionString:" + positionString + " i2:" + i2 + " a:" + a + " i3:" + i3 + " i4:" + i4 + " DueTime:" + dueTime + " i5:" + i5 + " i6:" + i6 + " ClientVer.:" + clientVersion + " lenghtRest:" + lenghtRest
                + " b:" + b + " i8:" + i8 + " c" + c + " i9:" + i9 + " Uhrzeit Client:" + clientStarted + " LenghtEncrypted:" + lenghtEncrypted);

            //byte[] sessionKey = HelperMethods.Instance.extractSessionKeyFrom1stClientPacket(simpleBlob);

            byte[] decrypted = HelperMethods.Instance.RC4ToBytes(encrypted);

            System.Diagnostics.Debug.WriteLine("Decrypted with Session Key:");
            for (int i = 0; i < decrypted.Length; i++)
                System.Diagnostics.Debug.Write((char)decrypted[i]);


            FileStream fsInput = new FileStream(@packetName + "0", FileMode.Open);
            answerData = new byte[fsInput.Length];
            fsInput.Read(answerData, 0, (int)fsInput.Length);
            fsInput.Close();

            udp.Send(answerData, answerData.Length, "127.0.0.1", iep.Port);

            return answerData;

        }

        private byte[] handleSecondPacket(BinaryReader br, UdpClient udp, IPEndPoint iep)
        {
            byte[] answerData = null;

            //ushort lengthBlock = br.ReadUInt16(); // length data-block
			//uint i2 = br.ReadUInt32(); // 00 00 08 00 don't know, maybe indicator what kind of (packet)action
			//uint i3 = br.ReadUInt32(); // 00 00 00 00 sequence number
			//byte[] sessionToken = br.ReadBytes(8); // could be
			//byte[] dataBlock = br.ReadBytes(lengthBlock); // needs to seperated further, two following zeros divides maybe the data
      
            // fire them all only to packet 33, because 34 fires the connect to the "join world server" in my packet constellation
            for (int i = 1; i < 5; i++)
            {
                FileStream fsInput = new FileStream(@packetName + i, FileMode.Open);
                answerData = new byte[fsInput.Length];
                fsInput.Read(answerData, 0, (int)fsInput.Length);
                fsInput.Close();
                udp.Send(answerData, answerData.Length, "127.0.0.1", iep.Port);
           }

            return answerData;

        }

        private byte[] handleSessionPacket(BinaryReader br, UdpClient udp, IPEndPoint iep)
        {
            byte[] answerData = null;

            // These are encrypted with...?
            byte[] packet = br.ReadBytes((int)br.BaseStream.Length);

            byte[] completePacket = new byte[packet.Length+2];

            Buffer.BlockCopy(clientSessionStartBytes, 0, completePacket, 0, 2);
            Buffer.BlockCopy(packet, 0, completePacket, 2, packet.Length);

            lock (this)
            {
                // original packet
                FileStream fsOutputNormal = new FileStream(@packetNameOutputOrg + numberClientPacket, FileMode.Create);

                fsOutputNormal.Write(completePacket, 0, completePacket.Length);

                fsOutputNormal.Close();
                fsOutputNormal.Dispose();
            }

            // decrypted packet with session key
            // wrong idea! they aren't rc4 enc, use table look-up
            FileStream fsOutputDecrypted= new FileStream(@packetNameOutputDec + numberClientPacket, FileMode.Create);

            lock(this)
            {
            packet = decryptClientPacket.generateDecryptedPacket(completePacket, true);
            }

            // packet parse
            byte[] sixBytes = new byte[6];

            Buffer.BlockCopy(packet,2,sixBytes,0,6);
            
            //
            // from here: please change to your needs
            //
            byte[] packet7 = {0xC0,0x01,0x06,0x00,0x00,0x00};
            byte[] packet8 = { 0x0F,0x00,0x06,0x00,0x00,0x00 };
            byte[] packet9 = { 0x19,0x00,0x06,0x00,0x00,0x08};

            if(HelperMethods.Instance.ArraysEqual(sixBytes, packet7))
            {

                FileStream fsInput = new FileStream(@packetName + "5", FileMode.Open);
                answerData = new byte[fsInput.Length];
                fsInput.Read(answerData, 0, (int)fsInput.Length);
                fsInput.Close();
                udp.Send(answerData, answerData.Length, "127.0.0.1", iep.Port);

            }

            if (HelperMethods.Instance.ArraysEqual(sixBytes, packet8))
            {

                for (int i = 6; i < 16; i++)
                {
                    FileStream fsInput = new FileStream(@packetName + i, FileMode.Open);
                    answerData = new byte[fsInput.Length];
                    fsInput.Read(answerData, 0, (int)fsInput.Length);
                    fsInput.Close();
                    udp.Send(answerData, answerData.Length, "127.0.0.1", iep.Port);
                }

            }

            if (HelperMethods.Instance.ArraysEqual(sixBytes, packet9))
            {

               // no time til now

            }


            fsOutputDecrypted.Write(packet, 0, packet.Length);

            fsOutputDecrypted.Close();
            fsOutputDecrypted.Dispose();

            numberClientPacket++;

            return answerData;

        }

        private byte[] paketAuswerten(byte[] data, UdpClient udp, IPEndPoint iep)
        {
            MemoryStream ms = new MemoryStream(data);
            BinaryReader br = new BinaryReader(ms, Encoding.UTF8);

            byte[] answer = null;

            byte[] startBytes;

            if (!inSession)
            {

                startBytes = new byte[4];

                // change some day to switch case

                Buffer.BlockCopy(data, 0, startBytes, 0, 4);

                bool firstClientPacket = HelperMethods.Instance.ArraysEqual(startBytes, firstClientPacketBytes);

                if (firstClientPacket)
                {
                    // skip the 4 bytes
                    br.ReadUInt16(); // 0x00 0x00
                    br.ReadUInt16(); // 0x00 0x00

                    answer = handleClientAuth(br, udp, iep);

                    // saves first client packet
                    FileStream fsOutputNormal = new FileStream(@packetNameOutputOrg + "0", FileMode.Create);

                    fsOutputNormal.Write(data, 0, data.Length);

                    fsOutputNormal.Close();
                    fsOutputNormal.Dispose();

                }
                else
                {
                    startBytes = new byte[2];
                    Buffer.BlockCopy(data, 0, startBytes, 0, 2);

                    bool secondClientPacket = HelperMethods.Instance.ArraysEqual(startBytes, secondClientPacketBytes);

                    if (secondClientPacket)
                    {
                        // skip the 2 bytes
                        br.ReadUInt16(); // 0x00 0x00
                        br.Read(clientSessionStartBytes, 0, 2); // get the start bytes of every client packet

                        // handles the second packet
                        answer = handleSecondPacket(br, udp, iep);

                        inSession = true;

                        // saves second client packet
                        FileStream fsOutputNormal = new FileStream(@packetNameOutputOrg + "1", FileMode.Create);

                        fsOutputNormal.Write(data, 0, data.Length);

                        fsOutputNormal.Close();
                        fsOutputNormal.Dispose();
                    }                

                }
            }
            else
            {
                startBytes = new byte[2];
                Buffer.BlockCopy(data, 0, startBytes, 0, 2);

                bool areSessionStartBytes = HelperMethods.Instance.ArraysEqual(startBytes, clientSessionStartBytes);

                 // from here rc4 encrypted client packets
                if (areSessionStartBytes)
                {
                    // skip the 2 bytes
                    br.ReadUInt16(); // 0xZZ 0xZZ

                    // handles the session packet
                    answer = handleSessionPacket(br, udp, iep);
                }
                else
                {
                    // Must be new session on other server, e.g. after joining the world

                }

            }


            br.Close();
            ms.Close();         

            return answer;
        }      

        // seperate thread for the join world login, not working atm
        public void run()
        {
            UdpClient client2 = new UdpClient(9001);


            //byte[] dataX = prg.ReadFully("terminate_connection.bin",-1);

            //UdpClient sender = new UdpClient();
            //sender.Connect();
            //sender.Send(dataX, dataX.Length, "74.201.107.43", 9003);

            client2.DontFragment = true;

            bool isRunning = true;

            //prg.joinWorld(prg.client, prg.anyIP);
            while (isRunning)
            {

                try
                {

                    //IPEndPoint anyIP2 = new IPEndPoint(IPAddress.Any, 0);
                    //byte[] data = client2.Receive(ref anyIP2);

                }
                catch (IOException e)
                {
					Console.WriteLine(e.ToString());
					System.Diagnostics.Debug.WriteLine(e);
                }

            }
        }

        static void Main(string[] args)
        {

            Program prg = new Program();

            // second server
            Thread t = new Thread(prg.run);
            t.Start();

            // first server
            prg.client = new UdpClient(9000);

            //UdpClient sender = new UdpClient();

            prg.client.DontFragment = true;

            bool isRunning = true;

            while (isRunning)
            {

                try
                {

                    prg.anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = prg.client.Receive(ref prg.anyIP);

                    byte[] answerData = null;

                    answerData = prg.paketAuswerten(data, prg.client, prg.anyIP);

                    if (answerData == null)
                    {


                    }
                    else
                    {
                       // prg.client.Send(answerData, answerData.Length, "127.0.0.1", prg.anyIP.Port);
                    }


                }
                catch (Exception err)
                {
                    Console.WriteLine(err.ToString());
                    System.Diagnostics.Debug.WriteLine(err);
                }
            }

            prg.client.Close();
        }

    }
}
