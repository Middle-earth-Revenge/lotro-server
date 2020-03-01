using System;
using SharpPcap;
using PacketDotNet;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Helper;

namespace LOTROPacketCaptureAndAutoDecryption
{
    class Program
    {

        IPAddress localIPAdress;
        Decrypt decryptPacket;

		readonly string pathOutputDecryptedPackets = "decrypted_packets" + Path.DirectorySeparatorChar;
		readonly string pathOutputOriginalPackets = "original_packets" + Path.DirectorySeparatorChar;
		int packetCounter;

        public Program()
        {
            decryptPacket = new Decrypt();
            localIPAdress = getLocalIp();
        }

        void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

			var udpPacket = packet.Extract<UdpPacket>();
            if (udpPacket != null)
            {
                //DateTime time = e.Packet.Timeval.Date;
                //int len = e.Packet.Data.Length;

                var ipPacket = udpPacket.ParentPacket as IPPacket;

                byte[] data = udpPacket.PayloadData;

                lock(this)
                {
                    //Console.WriteLine("SRC: " + localIPAdress + " vs. " + ipPacket.SourceAddress + ":" + udpPacket.SourcePort);
                    //Console.WriteLine("DST: " + localIPAdress + " vs. " + ipPacket.DestinationAddress + ":" + udpPacket.DestinationPort);
                    //Console.WriteLine();
                    string postfix;
                    bool isClientPacket;
                    if (ipPacket.SourceAddress.Equals(localIPAdress))
                    {
                        postfix = "_client";
                        isClientPacket = true;
                    }
                    else
                    {
                        postfix = "_server";
                        isClientPacket = false;
                    }

                    // decrypt packets
                    byte[] decryptedPacket = decryptPacket.GenerateDecryptedPacket(data, isClientPacket);

                    postfix += "-" + BitConverter.ToString(decryptedPacket, 4, 4).Replace("-", "");

                    using (FileStream fsOutput = new FileStream(@pathOutputDecryptedPackets + string.Format("{0,4:0000}", packetCounter) + postfix, FileMode.Create))
                    {
                        fsOutput.Write(decryptedPacket, 0, decryptedPacket.Length);
                        fsOutput.Close();
                    }

                    using (FileStream fsOutput = new FileStream(pathOutputOriginalPackets + string.Format("{0,4:0000}", packetCounter) + postfix, FileMode.Create))
                    {
                        fsOutput.Write(data, 0, data.Length);
                        fsOutput.Close();
                    }

                    packetCounter++;
                }
            }
        }

        static IPAddress getLocalIp()
        {
            IPAddress localIP = null;

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
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

            // Retrieve the device list
            CaptureDeviceList devices = CaptureDeviceList.Instance;

            ICaptureDevice device = null;
            if (devices.Count < 1)
            {
                Console.WriteLine("No devices were found on this machine.");
                return;
            }
			if (devices.Count == 1)
			{
				device = devices[0];
			}
			else
			{

				Console.WriteLine("The following " + devices.Count + " devices are available on this machine");
				Console.WriteLine("-----------------------------------------------------");
				Console.WriteLine();

				// Print out the available network devices
				int i = 0;
				foreach (ICaptureDevice dev in devices)
				{
					Console.WriteLine("[{0}.]\n{1}", i, dev);
					i++;
				}

				Console.Write("Please choose one (0-{0}): ", (i - 1));
				ConsoleKeyInfo c = Console.ReadKey();

				if (c.KeyChar == '\0')
				{
					Console.WriteLine("Invalid input, choosing first device");
					device = devices[0];
				}
				else
				{
					// Extract a device from the list
					device = devices[int.Parse(c.KeyChar.ToString())]; // this is my device
				}
            }
			Console.WriteLine(device);

			// Register our handler function to the
			// 'packet arrival' event
			device.OnPacketArrival += prg.device_OnPacketArrival;

            // Open the device for capturing
            const int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Normal, readTimeoutMilliseconds);

            // port 2900 is chat server, don't want these packets
            const string filter = "!broadcast and !multicast and udp and !port 53 and !port 59511 and !port 161 and !port 2900 and !port 5355";
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
