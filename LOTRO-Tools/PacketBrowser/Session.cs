using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketBrowser
{
    // Single packet recording
    public class Session
    {
        public Session()
        {
            Packets = new ObservableCollection<PacketData>();
        }

        // Packet data in this session
        public ObservableCollection<PacketData> Packets
        {
            get;
            private set;
        }

        // The source folder where the data for this session reside
        public string SourceFolder
        {
            get { return m_SourceFolder; }
            set
            {
                m_SourceFolder = value;
                Initialize();
            }
        }
        private string m_SourceFolder;

        public override string ToString()
        {
            return SourceFolder;
        }

        // Parse initial packet data from the source folder
        private void Initialize()
        {
            if (Directory.Exists(SourceFolder))
            {
                foreach (string packetFile in Directory.EnumerateFiles(SourceFolder).Where(f => string.IsNullOrEmpty(Path.GetExtension(f))))
                {
                    PacketData packet = CreatePacket(packetFile);
                    if (packet != null)
                    {
                        Packets.Add(packet);
                    }
                }
            }
        }

        // Create packet wrapper from file
        private PacketData CreatePacket(string file)
        {
            // Packets are expected in the following format:
            //  XXXX_client/server-ID
            string fileName = Path.GetFileNameWithoutExtension(file);

            try
            {
                int underscoreSep = fileName.IndexOf('_');
                int packetId = int.Parse(fileName.Substring(0, underscoreSep));
                string clientServer = fileName.Substring(underscoreSep + 1, 6); // Both 'client' and 'server' are 6 chars long :)
                uint type = Convert.ToUInt32(fileName.Substring(underscoreSep + 8, 8), 16);

                if (clientServer == "client")
                {
                    return new ClientPacket(file, packetId, type);
                }
                else if (clientServer == "server")
                {
                    return new ServerPacket(file, packetId, type);
                }

                // BS name
                return null;
            }
            catch (Exception)
            {
                // Something went terribly wrong
                return null;
            }
        }
    }
}
