using System;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;


/* 
 
 * To disable checksum check at "3B 4C 24 1C 75 6A 8B"
 * Replace 75 6a with 90 90
 
 */

namespace Settings
{
    //[DataContract]
    [Serializable]
    public sealed class Config
    {
        private static volatile Config instance;
        private static object syncRoot = new Object();

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

        internal void init()
        {
            if (this.Debug)
            {
                createFolder(LogFolder + "\\" + ClientLogFolder);
                createFolder(LogFolder + "\\" + ServerLogFolder);
            }

            createFolder(AccountFolder);
        }

        private void createFolder(string folderName)
        {
            DirectoryInfo directoryInfo = Directory.CreateDirectory(@folderName);
        }

        [XmlElement("Debug")]
        public bool Debug
        {
            get;
            set;
        }

        [XmlElement("DebugLogFile")]
        public string DebugLogFile
        {
            get;
            set;
        }

        [XmlElement("LogFolder")]
        public string LogFolder
        {
            get;
            set;
        }

        [XmlElement("ClientLogFolder")]
        public string ClientLogFolder
        {
            get;
            set;
        }

        [XmlElement("ServerLogFolder")]
        public string ServerLogFolder
        {
            get;
            set;
        }

        [XmlElement("RequiredClientVersion")]
        public string RequiredClientVersion
        {
            get;
            set;
        }

        [XmlElement("ServerName")]
        public string ServerName
        {
            get;
            set;
        }

        [XmlElement("ServerId")]
        public UInt16 ServerId
        {
            get;
            set;
        }

        [XmlElement("ServerPort")]
        public UInt16 ServerPort
        {
            get;
            set;
        }

        [XmlElement("WorldServerIP")]
        public string WorldServerIP
        {
            get;
            set;
        }

        [XmlElement("WorldServerPort")]
        public UInt16 WorldServerPort
        {
            get;
            set;
        }

        [XmlElement("AccountFolder")]
        public string AccountFolder
        {
            get;
            set;
        }

    }
}
