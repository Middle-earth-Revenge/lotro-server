using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using Helper;
using System.Collections;





namespace LOTROE2012
{

    class Program
    {

        private ConsoleKeyInfo cki;

        static void Main(string[] args)
        {
            Console.Clear();
            Console.TreatControlCAsInput = true;

            bool configFound = false;

            if (args.Length == 0)
            {
                Console.WriteLine("Missing config file name!");
                Console.WriteLine("Please start the server with a valid config file.");
                Console.WriteLine();
                Console.WriteLine("LOTRO-SE.exe {config file}");
                Console.WriteLine();
                Console.WriteLine("e.g. > LOTRO-SE.exe config.xml");
                Console.ReadKey(true);
                Environment.Exit(1);
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Config file {0} not found!", args[0]);
                Console.ReadKey(true);
                Environment.Exit(2);
            }

            Settings.ConfigHandler configHandler = new Settings.ConfigHandler();
            configHandler.readConfig(args[0]);

            if (Settings.Config.Instance.Debug)
            {
                TextWriterTraceListener tr1 = new TextWriterTraceListener(System.Console.Out);
                Debug.Listeners.Add(tr1);

                TextWriterTraceListener tr2 = new TextWriterTraceListener(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.DebugLogFile);
                Debug.Listeners.Add(tr2);
            }

            Program prg = new Program();

            if (Settings.Config.Instance != null)
                configFound = true;

            if (!configFound)
            {
                System.Console.WriteLine("\r\nCould not start server. There is a problem with the config file.");
                Console.ReadKey(true);
                Environment.Exit(0);
            }

            System.Console.WriteLine("* Debug: " + Settings.Config.Instance.Debug );
            System.Console.WriteLine("* Logfile: '" + System.IO.Directory.GetCurrentDirectory().ToString() + "\\" + Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.DebugLogFile + "'");
            System.Console.WriteLine("* Starting server [" + Settings.Config.Instance.ServerName + "] on port " + Settings.Config.Instance.ServerPort + "...");

            bool isListening = Server.UdpServer.Instance.startServer();

            

            if (isListening)
            {
                System.Console.WriteLine("* [" + Settings.Config.Instance.ServerName + "] is now listening @ " + Server.UdpServer.Instance.ServerAddress + "...");
                System.Console.WriteLine("\r\nExit server with CTRL+X...");
                System.Console.WriteLine();

                Debug.WriteLineIf(Settings.Config.Instance.Debug, "Server running", DateTime.Now.ToString() + " " + prg.GetType().Name + ".Main");

                bool noCTRLC = true;

                while (noCTRLC)
                {
                    prg.cki = Console.ReadKey(true);

                    if (prg.cki.Modifiers == ConsoleModifiers.Control && prg.cki.Key == ConsoleKey.X)
                    {
                        Server.UdpServer.Instance.stopServer();
                        noCTRLC = false;

                        System.Console.WriteLine("Server stopped...");
                        System.Console.WriteLine("Exit.");
                    }
                }
            }
            else
            {
                System.Console.WriteLine("Something went wrong...");
                System.Console.WriteLine("Exit.");
            }

            Debug.Close();
        }       
    }
}
