using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using Helper;
using System.Collections;
using System.Reflection;





namespace LOTROE2012
{

    class Program
    {
        public static void Main(string[] args)
        {
            Console.Clear();
            Console.TreatControlCAsInput = true;

            // Check if the programm was called properly
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

            // Check if the passed file exists (we assume that it's a file and readable)
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Config file {0} not found!", args[0]);
                Console.ReadKey(true);
                Environment.Exit(2);
            }

            // Load the config.xml into Settings.Config.Instance
            Settings.ConfigHandler configHandler = new Settings.ConfigHandler();
            configHandler.readConfig(args[0]);

            // Verify the config.xml was valid
            if (Settings.Config.Instance == null)
            {
                Console.WriteLine("Could not start server. There is a problem reading the config file {0}.", args[0]);
                Console.ReadKey(true);
                Environment.Exit(3);
            }

            // Give some basic information
            Console.WriteLine("{0} {1}", Assembly.GetEntryAssembly().GetName().Name, Assembly.GetEntryAssembly().GetName().Version);
            Console.WriteLine();
            Console.WriteLine("* Debug logging: {0}", Settings.Config.Instance.Debug);
            Console.WriteLine("* Logfile: '{0}\\{1}\\{2}'", System.IO.Directory.GetCurrentDirectory().ToString(), Settings.Config.Instance.LogFolder, Settings.Config.Instance.DebugLogFile);
            Console.WriteLine("* Starting server '{0}' on port {1}", Settings.Config.Instance.ServerName, Settings.Config.Instance.ServerPort + "...");

            // Set up the debugger. From now on no more calls to System.Console for debugging output!
            if (Settings.Config.Instance.Debug)
            {
                TextWriterTraceListener listener1 = new TextWriterTraceListener(System.Console.Out);
                Debug.Listeners.Add(listener1);

                TextWriterTraceListener listener2 = new TextWriterTraceListener(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.DebugLogFile);
                Debug.Listeners.Add(listener2);
            }

            // Test the Helper works
            Helper.HelperMethods.Instance.GetHashCode();

            bool isListening = Server.UdpServer.Instance.startServer();
            if (isListening)
            {
                Console.WriteLine("* Started '" + Settings.Config.Instance.ServerName + "'  is now listening...");
                Console.WriteLine();
                Console.WriteLine("Exit server with CTRL+X...");
                Console.WriteLine();

                Debug.WriteLineIf(Settings.Config.Instance.Debug, "Server running", DateTime.Now.ToString() + " " + "Program.Main");

                bool noCTRLC = true;
                while (noCTRLC)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    if (cki.Modifiers == ConsoleModifiers.Control && cki.Key == ConsoleKey.X)
                    {
                        Server.UdpServer.Instance.stopServer();
                        noCTRLC = false;

                        Console.WriteLine("Server stopped...");
                        Console.WriteLine("Exiting.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Something went wrong...");
                Console.WriteLine("Exiting.");
            }

            Debug.Close();
        }       
    }
}
