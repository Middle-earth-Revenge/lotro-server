using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PacketBrowser
{
    // Packet segment translation types
    public enum PacketSegmentType : uint
    {
        Unknown = 0,
        UnsignedInteger = 1,
        SignedInteger = 2,
        FloatingPoint = 3,
        ASCII = 4,
        Unicode = 5
    }

    // A single data segment inside a packet
    public class PacketSegment
    {
        public PacketSegment(PacketData packet, int offset, int length)
        {
            Packet = packet;
            Offset = offset;
            Length = length;

            // Default values
            TranslationType = PacketSegmentType.Unknown;
            Color = Colors.WhiteSmoke;
            StringLengthSize = 1;
        }

        // The parent packet
        public PacketData Packet
        {
            get;
            private set;
        }
        
        // Segment start offset
        public int Offset
        {
            get;
            private set;
        }

        // Segment length
        public int Length
        {
            get
            {
                // Some types override the length
                if (TranslationType == PacketSegmentType.ASCII)
                {
                    return StringLength + StringLengthSize;
                }
                else if (TranslationType == PacketSegmentType.Unicode)
                {
                    return (StringLength * 2) + StringLengthSize;
                }
                else
                {
                    // Other types should be user set
                    return m_Length;
                }
            }
            private set { m_Length = value; }
        }
        private int m_Length;

        // The size of the length field for strings
        public int StringLengthSize
        {
            get;
            set;
        }

        public int StringLength
        {
            get
            {
                if (StringLengthSize == 1)
                {
                    return (int)Packet.RawData[Offset];
                }
                else if (StringLengthSize == 2)
                {
                    return (int)CustomBitConverter.ToUInt16(Packet.RawData, Offset);
                }
                else if (StringLengthSize == 4)
                {
                    return (int)CustomBitConverter.ToUInt32(Packet.RawData, Offset);
                }
                else
                {
                    // Bogus?
                    return 0;
                }
            }
        }

        // Little endian flag
        public bool IsLittleEndian
        {
            get;
            set;
        }

        // Readable name of the segment (if any)
        public string Name
        {
            get;
            set;
        }

        // Color for the UI (if any)
        public Color Color
        {
            get;
            set;
        }

        // Translation type for this segment
        public PacketSegmentType TranslationType
        {
            get;
            set;
        }

        // The value of the segment
        public object Value
        {
            get
            {
                switch (TranslationType)
                {
                    case PacketSegmentType.SignedInteger:
                        if (Length == 1)
                        {
                            return (sbyte)Packet.RawData[Offset];
                        }
                        else if (Length == 2)
                        {
                            return CustomBitConverter.ToInt16(Packet.RawData, Offset, !IsLittleEndian);
                        }
                        else if (Length == 4)
                        {
                            return CustomBitConverter.ToInt32(Packet.RawData, Offset, !IsLittleEndian);
                        }
                        else if (Length == 8)
                        {
                            return CustomBitConverter.ToInt64(Packet.RawData, Offset, !IsLittleEndian);
                        }
                        break;

                    case PacketSegmentType.UnsignedInteger:
                        if (Length == 1)
                        {
                            return Packet.RawData[Offset];
                        }
                        else if (Length == 2)
                        {
                            return CustomBitConverter.ToUInt16(Packet.RawData, Offset, !IsLittleEndian);
                        }
                        else if (Length == 4)
                        {
                            return CustomBitConverter.ToUInt32(Packet.RawData, Offset, !IsLittleEndian);
                        }
                        else if (Length == 8)
                        {
                            return CustomBitConverter.ToUInt64(Packet.RawData, Offset, !IsLittleEndian);
                        }
                        break;

                    case PacketSegmentType.FloatingPoint:
                        if (Length == 4)
                        {
                            return CustomBitConverter.ToSingle(Packet.RawData, Offset, !IsLittleEndian);
                        }
                        else if (Length == 8)
                        {
                            return CustomBitConverter.ToDouble(Packet.RawData, Offset, !IsLittleEndian);
                        }
                        break;

                    case PacketSegmentType.ASCII:
                        return System.Text.Encoding.ASCII.GetString(Packet.RawData, Offset + StringLengthSize, StringLength);

                    case PacketSegmentType.Unicode:
                        if (IsLittleEndian)
                        {
                            return System.Text.Encoding.Unicode.GetString(Packet.RawData, Offset + StringLengthSize, StringLength * 2);
                        }
                        else
                        {
                            return System.Text.Encoding.BigEndianUnicode.GetString(Packet.RawData, Offset + StringLengthSize, StringLength * 2);
                        }
                }

                // Unknown value
                return "Unknown";
            }
        }
    }
}
