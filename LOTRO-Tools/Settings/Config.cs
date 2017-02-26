using System;
using System.IO;
using System.Xml.Serialization;


/*
 *
 * To disable checksum check at "3B 4C 24 1C 75 6A 8B"
 * Replace 75 6a with 90 90
 *
 */
namespace Settings
{
    [Serializable]
    public sealed class Config
    {
        static volatile Config instance;
        static object syncRoot = new Object();

        internal Config()
        {

        }

        public static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new Config();
                        }
                    }
                }

                return instance;
            }

            internal set { instance = value; }

        }

        /// <summary>
        /// Create necessary directories that will be returned by this class 
        /// </summary>
        internal void init()
        {
            if (Debug)
            {
                createFolder(LogFolder + "\\" + ClientLogFolder);
                createFolder(LogFolder + "\\" + ServerLogFolder);
            }

            createFolder(AccountFolder);
        }

        /// <summary>
        /// Create a directory and verify it succeeded
        /// </summary>
        /// <param name="folderName">The directory to create</param>
        static void createFolder(string folderName)
        {
            DirectoryInfo directoryInfo = Directory.CreateDirectory(@folderName);
            if (!directoryInfo.Exists)
            {
                throw new IOException("Could not create directory " + folderName);
            }
        }

        /// <summary>
        /// If set to true the server will log information to the console and also store sent/received packets
        /// </summary>
        [XmlElement("Debug")]
        public bool Debug
        {
            get;
            set;
        }

        /// <summary>
        /// File (created in the startup directory) which will receive any debug output that also ends up on the console
        /// </summary>
        [XmlElement("DebugLogFile")]
        public string DebugLogFile
        {
            get;
            set;
        }

        /// <summary>
        /// Base folder for logged packets
        /// </summary>
        [XmlElement("LogFolder")]
        public string LogFolder
        {
            get;
            set;
        }

        /// <summary>
        /// Currently unused
        /// </summary>
        [XmlElement("ClientLogFolder")]
        public string ClientLogFolder
        {
            get;
            set;
        }

        /// <summary>
        /// Relative path (below LogFolder) to be used to store incoming/outgoing packets
        /// </summary>
        [XmlElement("ServerLogFolder")]
        public string ServerLogFolder
        {
            get;
            set;
        }

        /// <summary>
        /// Version identifaction string of the lotro client (e.g. "061004_netver:7563; didver:926CD8E3-2984-4CA9-9C6B-6DF2C8EB6BC3" to match the version from Q1 2013)
        /// </summary>
        [XmlElement("RequiredClientVersion")]
        public string RequiredClientVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Human readable name for this server instance, e.g. "[EN] Testserver" (currently only used in the debug output)
        /// </summary>
        [XmlElement("ServerName")]
        public string ServerName
        {
            get;
            set;
        }

        /// <summary>
        /// Unique ID of the server, currently set to 241
        /// </summary>
        [XmlElement("ServerId")]
        public UInt16 ServerId
        {
            get;
            set;
        }

        /// <summary>
        /// UDP port the server will be listening to
        /// </summary>
        [XmlElement("ServerPort")]
        public UInt16 ServerPort
        {
            get;
            set;
        }

        [XmlElement("MaxClients")]
        public UInt16 MaxClients
        {
            get;
            set;
        }

        /// <summary>
        /// Currently unused
        /// </summary>
        [XmlElement("WorldServerIP")]
        public string WorldServerIP
        {
            get;
            set;
        }

        /// <summary>
        /// Currently unused
        /// </summary>
        [XmlElement("WorldServerPort")]
        public UInt16 WorldServerPort
        {
            get;
            set;
        }

        /// <summary>
        /// Path of the folder containing the XML definitions for accounts
        /// </summary>
        [XmlElement("AccountFolder")]
        public string AccountFolder
        {
            get;
            set;
        }

    }
}