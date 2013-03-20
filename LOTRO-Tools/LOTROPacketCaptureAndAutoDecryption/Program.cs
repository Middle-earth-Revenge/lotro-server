using System;
using System.Collections.Generic;
using System.Text;
using SharpPcap;
using PacketDotNet;
using System.Net;
using System.Net.Sockets;
using System.IO;
using LOTRO;

namespace LOTROPacketCaptureAndAutoDecryption
{
    class Program
    {

        private IPAddress localIPAdress;
        private Decrypt decryptPacket;

		private readonly string pathOutputDecryptedPackets = "decrypted_packets" + Path.DirectorySeparatorChar;
		private readonly string pathOutputOriginalPackets = "original_packets" + Path.DirectorySeparatorChar;
        private Int32 packetCounter = 0;

        private void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

            var udpPacket = UdpPacket.GetEncapsulated(packet);
            if (udpPacket != null)
            {
                //DateTime time = e.Packet.Timeval.Date;
                //int len = e.Packet.Data.Length;

                var ipPacket = (PacketDotNet.IpPacket)udpPacket.ParentPacket;
                System.Net.IPAddress srcIp = ipPacket.SourceAddress;
                //System.Net.IPAddress dstIp = ipPacket.DestinationAddress;
                //int srcPort = udpPacket.SourcePort;
                //int dstPort = udpPacket.DestinationPort;

                byte[] data = udpPacket.PayloadData;

                lock(this)
                {

                    if (srcIp.Equals(localIPAdress))
                    {
                        // decrypt client packets
                        byte[] decryptedClientPacket = decryptPacket.GenerateDecryptedPacket(data, true);

                        using (FileStream fsOutput = new FileStream(@pathOutputDecryptedPackets + String.Format("{0,4:0000}", packetCounter) + "_client", FileMode.Create))
                        {
                            fsOutput.Write(decryptedClientPacket, 0, decryptedClientPacket.Length);
                            fsOutput.Close();
                        }

                        using (FileStream fsOutput = new FileStream(pathOutputOriginalPackets + String.Format("{0,4:0000}", packetCounter) + "_client", FileMode.Create))
                        {
                            fsOutput.Write(data, 0, data.Length);
                            fsOutput.Close();
                        }
                    }
                    else
                    {
                        // decrypt server packets
                        byte[] decryptedServerPacket = decryptPacket.GenerateDecryptedPacket(data, false);

                        using (FileStream fsOutput = new FileStream(@pathOutputDecryptedPackets + String.Format("{0,4:0000}", packetCounter) + "_server", FileMode.Create))
                        {
                            fsOutput.Write(decryptedServerPacket, 0, decryptedServerPacket.Length);
                            fsOutput.Close();
                        }

                        using (FileStream fsOutput = new FileStream(pathOutputOriginalPackets + String.Format("{0,4:0000}", packetCounter) + "_server", FileMode.Create))
                        {
                            fsOutput.Write(data, 0, data.Length);
                            fsOutput.Close();
                        }
                    }

                    packetCounter++;

                }
            }
        }

        private IPAddress getLocalIp()
        {
            IPHostEntry host;
            //string localIP = "?";
            IPAddress localIP = null;

            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip;
                    break;
                }
            }

            return localIP;
        }

        static void Main(string[] args)
        {

            Program prg = new Program();

            prg.decryptPacket = new Decrypt();

            prg.localIPAdress = prg.getLocalIp();

            // Retrieve the device list
            CaptureDeviceList devices = CaptureDeviceList.Instance;

            if (devices.Count < 1)
            {
                Console.WriteLine("No devices were found on this machine");
                return;
            }

            Console.WriteLine("\nThe following devices are available on this machine. Please choose one:");
            Console.WriteLine("----------------------------------------------------\n");

            // Print out the available network devices
            int i = 0;
            foreach (ICaptureDevice dev in devices)
            {
                Console.WriteLine(i + ". {0}\n", dev);
                i++;
            }

            ConsoleKeyInfo c = Console.ReadKey();

            // Extract a device from the list
            ICaptureDevice device = devices[Int32.Parse(c.KeyChar.ToString())]; // this is my device           

            // Register our handler function to the
            // 'packet arrival' event
            device.OnPacketArrival +=
                new SharpPcap.PacketArrivalEventHandler(prg.device_OnPacketArrival);

            // Open the device for capturing
            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Normal, readTimeoutMilliseconds);

            // port 2900 is chat server, don't want these packets
            string filter = "!broadcast and !multicast and udp and !port 53 and !port 59511 and !port 161 and !port 2900";
            device.Filter = filter;

            Directory.CreateDirectory(prg.pathOutputDecryptedPackets);
            Directory.CreateDirectory(prg.pathOutputOriginalPackets);

            // Start the capturing process
            device.StartCapture();

            Console.WriteLine("\nCapturing has started. Hit 'Enter' to exit application. Don't forget to look inside the 'decrypted_packets' folder");
            // Wait for 'Enter' from the user.
            Console.ReadLine();

            // Stop the capturing process
            device.StopCapture();

            // Close the pcap device
            device.Close();
        }
    }
}
