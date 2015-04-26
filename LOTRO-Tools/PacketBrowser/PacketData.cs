using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketBrowser
{
    // Base for client & server packets
    public abstract class PacketData
    {
        public PacketData(string file, int id, uint type)
        {
            FileName = file;
            Index = id;
            Type = type;

            // Load payload
            if (File.Exists(FileName))
            {
                RawData = File.ReadAllBytes(FileName);
            }
        }

        // Packet source file
        public string FileName
        {
            get;
            private set;
        }

        // Packet index in the sequence
        public int Index
        {
            get;
            private set;
        }

        // Packet type
        public uint Type
        {
            get;
            private set;
        }

        // Packet payload data
        public byte[] RawData
        {
            get;
            private set;
        }

        // Packet timestamp
        public string Timestamp
        {
            get { return new FileInfo(FileName).CreationTime.ToString("HH:mm:ss.ffff"); }
        }

        // Packet info summary
        public string Summary
        {
            get
            {
                if (RawData == null)
                {
                    return "No data";
                }
                else
                {
                    return RawData.Length + " bytes of data";
                }
            }
        }

        // Packet data
        public string TranslatedData
        {
            get
            {
                // The default implementation just returns the raw data
                if (RawData == null)
                {
                    return "[EMPTY]";
                }
                else
                {
                    string data = string.Empty;

                    data += "Raw:\n";
                    foreach (byte b in RawData)
                    {
                        data += b.ToString("X2") + " ";
                    }

                    data += "\n\nASCII:\n";
                    data += System.Text.Encoding.ASCII.GetString(RawData);

                    data += "\n\nUnicode:\n";
                    data += System.Text.Encoding.Unicode.GetString(RawData);

                    data += "\n\nBig endian Unicode:\n";
                    data += System.Text.Encoding.BigEndianUnicode.GetString(RawData);

                    return data;
                }

            }
        }

        // This is a funny way of handling the client/server columns :)
        public abstract PacketData ClientData
        {
            get;
        }
        public abstract PacketData ServerData
        {
            get;
        }
    }
}
