using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PacketBrowser
{
    // Packet analyzer that uses database entries to generate the segments
    class FixedPacketAnalyzer : IPacketAnalyzer
    {
        public FixedPacketAnalyzer(PacketDescription desc) 
        {
            Desc = desc;
        }

        public void AnalyzePacket(PacketData packet)
        {
            // Fill in basic info
            packet.Name = Desc.Name;
            packet.Comment = Desc.Comment;

            // Color table for coloring the segments
            Color[] colors = new Color[]
            {
                Colors.Aqua,
                Colors.Gold,
                Colors.LimeGreen,
                Colors.MediumSlateBlue,
                Colors.Coral,
                Colors.Crimson,
                Colors.DeepSkyBlue,
                Colors.Fuchsia
            };

            // Generate the segments
            int offset = packet.PayloadOffset;
            int colorIndex = 0;
            foreach (PacketSegmentDescription seg in Desc.Segments)
            {
                // If there is a gap between the last segment and this one, generate a stub one
                if (seg.RelativeOffset > 0)
                {
                    packet.Segments.Add(new PacketSegment(packet, offset, seg.RelativeOffset)
                    {
                        Name = string.Format("Unknown data blob ({0} bytes)", seg.RelativeOffset)
                    });
                }

                // Generate the current segment
                int absoluteOffset = offset + seg.RelativeOffset;
                PacketSegment segment = new PacketSegment(packet, absoluteOffset, seg.Length)
                {
                    Name = seg.Name,
                    TranslationType = seg.Type,
                    Color = colors[(colorIndex++) % colors.Length],
                    IsLittleEndian = seg.IsLittleEndian,
                    StringLengthSize = seg.StringLengthSize > 0 ? seg.StringLengthSize : 1
                };
                packet.Segments.Add(segment);

                // Update the offset
                offset = absoluteOffset + segment.Length;
            }

            // Remaining space
            if (packet.RawData.Length > offset)
            {
                int size = packet.RawData.Length - offset;
                packet.Segments.Add(new PacketSegment(packet, offset, size)
                {
                    Name = string.Format("Unknown data blob ({0} bytes)", size)
                });
            }
        }

        // The loaded packet description
        public PacketDescription Desc
        {
            get;
            private set;
        }
    }
}
