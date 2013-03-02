using System;
using System.Net;
using Protocol.Generic;

/* Class SocketObject
 * 
 * Contains:
 * 
 * raw buffer [max. 65535 bytes]
 * client endpoint [eg. 127.0.0.1:56789]
 * serialized payload (the data payload of the dgram packet contains two elements - a header and a data part)
 * 
*/


namespace Server
{
    public class SocketObject
    {
        private const int bufferSize = 65535; // max 65535
        private byte[] dataBuffer = new byte[bufferSize];

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
