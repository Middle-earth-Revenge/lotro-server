using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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
            Segments = new ObservableCollection<PacketSegment>();

            // Load payload
            if (File.Exists(FileName))
            {
                RawData = File.ReadAllBytes(FileName);
                InitSegments();
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
                    string str = Name;
                    if (!string.IsNullOrEmpty(str))
                        str += " ";

                    if (!string.IsNullOrEmpty(Comment))
                    {
                        str += " - " + Comment + " ";
                    }

                    str += "[" + RawData.Length + " bytes of data]";

                    return str;
                }
            }
        }

        // Human-readable packet name
        public string Name
        {
            get;
            set;
        }

        // User comment
        public string Comment
        {
            get;
            set;
        }

        // Segments defined for this packet
        public ObservableCollection<PacketSegment> Segments
        {
            get;
            private set;
        }

        // Retrieve segment by byte offset
        public PacketSegment GetSegmentForByte(int offset)
        {
            foreach (PacketSegment seg in Segments)
            {
                if (offset >= seg.Offset &&
                    offset < seg.Offset + seg.Length)
                {
                    return seg;
                }
            }

            return null;
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

        public int PayloadOffset
        {
            get
            {
                return HeaderOffset + 20;
            }
        }

        private int HeaderOffset
        {
            get
            {
                if (Type == 0x00010000)
                {
                    // Session init packet, skip first two bytes!
                    return 2;
                }

                return 0;
            }
        }

        // Initialize basic segments
        private void InitSegments()
        {
            // Init the header segments
            int baseOffset = 0;

            // Session startup segment for first packet
            if (Type == 0x00010000)
            {
                baseOffset = 2;
                Segments.Add(new PacketSegment(this, 0, 2) { Name = "First packet offset", TranslationType = PacketSegmentType.UnsignedInteger });
            }

            // Session ID
            int offset = baseOffset;
            Segments.Add(new PacketSegment(this, offset, 2) { Name = "Session ID", Color = Colors.LightGreen, TranslationType = PacketSegmentType.UnsignedInteger });
            offset += 2;

            // Payload size
            Segments.Add(new PacketSegment(this, offset, 2) { Name = "Payload size", Color = Colors.LightGoldenrodYellow, TranslationType = PacketSegmentType.UnsignedInteger });
            offset += 2;

            // Packet ID
            Segments.Add(new PacketSegment(this, offset, 4) { Name = "Packet type", Color = Colors.LightSteelBlue, TranslationType = PacketSegmentType.UnsignedInteger });
            offset += 4;

            // Packet sequence number
            Segments.Add(new PacketSegment(this, offset, 4) { Name = "Sequence number", Color = Colors.LightCyan, TranslationType = PacketSegmentType.UnsignedInteger });
            offset += 4;

            // Payload data checksum
            Segments.Add(new PacketSegment(this, offset, 4) { Name = "Payload checksum", Color = Colors.LightPink, TranslationType = PacketSegmentType.UnsignedInteger });
            offset += 4;

            // Temporary session number (???)
            Segments.Add(new PacketSegment(this, offset, 4) { Name = "Temp session number???", Color = Colors.LightBlue, TranslationType = PacketSegmentType.UnsignedInteger });
            offset += 4;

            // Generate payload segments
            if (!PacketDatabase.FillSegmentInfo(this))
            {
                // Data unknown
                Segments.Add(new PacketSegment(this, offset, RawData.Length - offset)
                {
                    Name = string.Format("Payload ({0} bytes)", RawData.Length - offset),
                    TranslationType = PacketSegmentType.UnsignedInteger
                });
            }
        }
    }
}
