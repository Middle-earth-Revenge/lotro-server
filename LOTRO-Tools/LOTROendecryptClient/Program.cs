using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LOTROendecryptClient
{
    class Program
    {

        private Decrypt packetDecrypt;
       // private Encrypt packetEncrypt;
 
        private FileStream fsInput;
        private FileStream fsOutput;

        private bool encrypting = false;

        static void Main(string[] args)
        {
            Program prg = new Program();

            prg.encrypting = false;

            bool argsOK = prg.parseArgs(args);

            if (!argsOK)
            {
                System.Console.WriteLine("Please start with: LOTROendecrypt -f <filename input> -m <en or de> -o <filename output>");
                System.Console.ReadKey();
                Environment.Exit(0);
            }


            // batch me
            
            /*DirectoryInfo di = new DirectoryInfo(@"packets");
            FileInfo[] fi = di.GetFiles();

            if (prg.fsInput != null)
                prg.fsInput.Close();

            if (prg.fsOutput != null)
                prg.fsOutput.Close();

            prg.encrypting = false;

            for (int i = 0; i < fi.Length; i++)
            {
                prg.fsInput = new FileStream(fi[i].FullName, FileMode.Open);
                prg.fsOutput = new FileStream(@"packets_out\\client_" + fi[i].Name, FileMode.Create); */

                switch (prg.encrypting)
                {
                    case false:
                        prg.packetDecrypt = new Decrypt();
				byte[] decryptedPacket = prg.packetDecrypt.generateDecryptedPacket(prg.fsInput);
                        prg.fsOutput.Write(decryptedPacket, 0, decryptedPacket.Length);
                        break;
                    case true:
                       // prg.packetEncrypt = new Encrypt(prg.fsInput);
                       // byte[] encryptedPacket = prg.packetEncrypt.generateEncryptedPacket();
                       // prg.fsOutput.Write(encryptedPacket, 0, encryptedPacket.Length);
                        break;

                }


                if (prg.fsInput != null)
                    prg.fsInput.Close();

                if (prg.fsOutput != null)
                    prg.fsOutput.Close();

           }

       // } // batch me end




        // parse the given args
        // creates FileStreams
        private bool parseArgs(string[] args)
        {
            bool argsOK = false;

            if (args.Length < 6 || args.Length > 6)
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
                    case "-m":
                        if (args[1].Equals("en"))
                            encrypting = true;
                        break;
                    case "-o":
                        fsOutput = new FileStream(@args[1], FileMode.Create);
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
                    case "-m":
                        if (args[3].Equals("en"))
                            encrypting = true;
                        break;
                    case "-o":
                        fsOutput = new FileStream(@args[3], FileMode.Create);
                        break;
                    default:
                        argsOK = false;
                        break;
                }

                switch (args[4])
                {
                    case "-f":
                        fsInput = new FileStream(@args[5], FileMode.Open);
                        break;
                    case "-m":
                        if (args[5].Equals("en"))
                            encrypting = true;
                        break;
                    case "-o":
                        fsOutput = new FileStream(@args[5], FileMode.Create);
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
