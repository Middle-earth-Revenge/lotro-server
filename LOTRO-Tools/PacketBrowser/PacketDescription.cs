using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketBrowser
{
    // Description of the packet in the database
    class PacketDescription
    {
        public PacketDescription()
        {
            //Segments = new List<PacketSegmentDescription>();
        }

        // Custom name of the packet
        public string Name
        {
            get;
            set;
        }

        // User comment on the packet
        public string Comment
        {
            get;
            set;
        }

        // Segment descriptions
        public List<PacketSegmentDescription> Segments
        {
            get;
            set;
        }
    }
}
