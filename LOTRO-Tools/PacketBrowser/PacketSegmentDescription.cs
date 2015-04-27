using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketBrowser
{
    // Description of a single packet segment in the database
    class PacketSegmentDescription
    {
        // Name of the segment
        public string Name
        {
            get;
            set;
        }

        // User comment on the segment
        public string Comment
        {
            get;
            set;
        }

        // Relative offset of the segment
        // This is the offset !from the last known previous segment!
        public int RelativeOffset
        {
            get;
            set;
        }

        // Length of the segment
        public int Length
        {
            get;
            set;
        }

        // The size of the length field for strings
        public int StringLengthSize
        {
            get;
            set;
        }

        // Endianness of the segment
        public bool IsLittleEndian
        {
            get;
            set;
        }

        // Type of the data in the segment
        public PacketSegmentType Type
        {
            get;
            set;
        }
    }
}
