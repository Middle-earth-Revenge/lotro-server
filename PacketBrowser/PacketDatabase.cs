using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketBrowser
{
    public static class PacketDatabase
    {
        static PacketDatabase()
        {
            //TEST:
            m_Analyzers[0x00010000] = new FixedPacketAnalyzer(new PacketDescription()
            {
                Name = "Session setup",
                Comment = "The 1st packet sent from the client to the server to establish connection",
                Segments = new List<PacketSegmentDescription>()
                {
                    new PacketSegmentDescription()
                    {
                        Name = "Client version",
                        RelativeOffset = 0,
                        Type = PacketSegmentType.ASCII,
                    },
                    new PacketSegmentDescription()
                    {
                        Name = "Timestamp",
                        RelativeOffset = 12,
                        Length = 4,
                        Type = PacketSegmentType.UnsignedInteger
                    },
                    new PacketSegmentDescription()
                    {
                        Name = "Account name",
                        RelativeOffset = 0,
                        Type = PacketSegmentType.Unicode,
                        IsLittleEndian = true
                    },
                    new PacketSegmentDescription()
                    {
                        Name = "Client public key",
                        RelativeOffset = 4,
                        Type = PacketSegmentType.ASCII
                    }
                }
            });
        }

        // Fill in segment info for a packet
        public static bool FillSegmentInfo(PacketData packet)
        {
            // Check if we have an analyzer for this type of packet
            IPacketAnalyzer analyzer;
            if (m_Analyzers.TryGetValue(packet.Type, out analyzer))
            {
                analyzer.AnalyzePacket(packet);
                return true;
            }

            // No handler
            return false;
        }

        // Registered packet analyzers
        private static Dictionary<uint, IPacketAnalyzer> m_Analyzers = new Dictionary<uint, IPacketAnalyzer>();
    }
}
