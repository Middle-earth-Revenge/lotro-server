using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CreatePacketsFromWiresharkHexDump
{
    class Program
    {
        private FileStream fsInput;
        private FileStream fsOutput;
        private string fsOutputName = "";
        private int packetsGenerated = 0;
        private int startByte = 0;

        static void Main(string[] args)
        {
            Program prg = new Program();

            bool argsOK = prg.parseArgs(args);

            if (!argsOK)
            {
                System.Console.WriteLine("Please start with: CreatePacketsFromWiresharkHexDump -f <wireshark udp dump filename input> -o <filename outputfolder>");
                System.Console.ReadKey();
                Environment.Exit(0);
            }

            MemoryStream ms = new MemoryStream();
            BufferedStream br = new BufferedStream(prg.fsInput);

           
            
            int firstByte = br.ReadByte();

            if (firstByte == 0)
            {
                ms.WriteByte((byte)firstByte);

                int secondByte = br.ReadByte();

                if (secondByte == 0)
                {
                    ms.WriteByte((byte)secondByte);

                    // gets head for following packets
                    prg.startByte = br.ReadByte();

                    ms.WriteByte((byte)prg.startByte);

                }

            }
            else
            {
                prg.startByte = firstByte;
                ms.WriteByte((byte)prg.startByte);
                ms.WriteByte((byte)0);
            }

            int tempByte = 0;
            int tempByte2 = 0;
            bool gotPacket = false;

            while (!gotPacket)
            {
                tempByte = br.ReadByte();

                if (tempByte != prg.startByte)
                    ms.WriteByte((byte)tempByte);
                else
                {
                    tempByte2 = br.ReadByte();

                    if (tempByte2 == 0)
                        gotPacket = true;
                    else
                    {
                        ms.WriteByte((byte)tempByte);
                        ms.WriteByte((byte)tempByte2);
                    }

                }
            }

            // the first packet of dump
            prg.fsOutput = new FileStream(@prg.fsOutputName + prg.packetsGenerated, FileMode.Create);

            prg.fsOutput.Write(ms.ToArray(), 0, (int)ms.Length);

            prg.packetsGenerated++;

            if (prg.fsOutput != null)
                prg.fsOutput.Close();

            

            while ((tempByte = br.ReadByte()) != -1)
            {
                gotPacket = false;

                prg.fsOutput = new FileStream(@prg.fsOutputName + prg.packetsGenerated, FileMode.Create);

                prg.fsOutput.WriteByte((byte)prg.startByte);

                prg.fsOutput.WriteByte(0);

                prg.fsOutput.WriteByte((byte)tempByte);

                while (!gotPacket)
                {
                    tempByte = br.ReadByte();

                    if (tempByte != -1)
                    {


                        if (tempByte != prg.startByte)
                            prg.fsOutput.WriteByte((byte)tempByte);
                        else
                        {
                            tempByte2 = br.ReadByte();

                            if (tempByte2 == 0)
                                gotPacket = true;
                            else
                            {
                                prg.fsOutput.WriteByte((byte)tempByte);
                                prg.fsOutput.WriteByte((byte)tempByte2);
                            }

                        }
                    }
                    else
                    {
                        gotPacket = true;
                    }

                }

                prg.packetsGenerated++;

                if (prg.fsOutput != null)
                    prg.fsOutput.Close();

            }




            if (prg.fsInput != null)
                prg.fsInput.Close();



        }

        private bool parseArgs(string[] args)
        {
            bool argsOK = false;

            if (args.Length < 4 || args.Length > 4)
            {
                argsOK = false;
            }

            argsOK = true;

            try
            {

                switch (args[0])
                {
                    case "-f":
                        fsInput = new FileStream(@args[1], FileMode.Open);
                        break;
                    case "-o":
                        fsOutputName = @args[3];
                        break;
                    default:
                        argsOK = false;
                        break;
                }

                switch (args[2])
                {
                    case "-f":
                        fsInput = new FileStream(@args[3], FileMode.Open);
                        break;
                    case "-o":
                        fsOutputName = @args[3];
                        break;
                    default:
                        argsOK = false;
                        break;
                }


            }
            catch (Exception e)
            {
                argsOK = false;
                System.Console.WriteLine(e.ToString());
            }

            return argsOK;
        }

    }
}
