using System;
using System.Net;
using Protocol.Generic;

namespace Server
{
    /// <summary>
    /// Contains:
    /// 
    /// raw buffer [max. 65535 bytes]
    /// client endpoint [eg. 127.0.0.1:56789]
    /// serialized payload (the data payload of the dgram packet contains two elements - a header and a data part)
    /// </summary>
    public class SocketObject
    {
        private const int MAX_BUFFER_SIZE = 65535;

        private byte[] dataBuffer = new byte[MAX_BUFFER_SIZE];
        public EndPoint EndPoint = new IPEndPoint(0, 0);

        public byte[] Buffer
        {
            get { return this.dataBuffer; }
            set { this.dataBuffer = value; }
        }

        public UInt16 Length
        {
            get;
            set;
        }
    }
}
