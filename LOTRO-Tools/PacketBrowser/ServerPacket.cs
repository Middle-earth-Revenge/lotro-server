using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketBrowser
{
    public class ServerPacket : PacketData
    {
        public ServerPacket(string file, int id, uint type)
            : base (file, id, type)
        { }

        public override PacketData ClientData
        {
            get { return null; }
        }
        public override PacketData ServerData
        {
            get { return this; }
        }
    }
}
