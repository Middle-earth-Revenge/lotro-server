﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Protocol.Generic;
using Helper;

namespace Protocol.SessionSetup
{
    public class Synchronize:PayloadData
    {
        // Client Data

        public string ClientVersion {get; set;}
        public DateTime LocalTimeStarted { get; set; }
        public UInt32 UnknownA { get; set; }
        public UInt32 HasGLSTicket { get; set; }
        public string AccountName { get; set; }
        public string GLSTicketDirect { get; set; }

        public override UInt16 Request
        {
            get {return 1;}
        }

        public override byte[] Response 
        {
            get { return new byte[] { 0x00, 0x00, 0x04, 0x00 }; }
        }

        //private byte[] sessionKey;
        //private byte[] accountData;

        // Server Data

        public Synchronize():base()
        {

        }

        public override byte[] Serialize(BEBinaryWriter beBinaryWriter)
        {
            throw new NotImplementedException("This should not be called");;
        }

        public override object Deserialize(BEBinaryReader ber)
        {
            try
            {
                ClientVersion = ber.ReadString();

                UInt32 lenNextPart = ber.ReadUInt32();

                UnknownA = ber.ReadUInt32();
                HasGLSTicket = ber.ReadUInt32();
                UInt32 timeStarted = ber.ReadUInt32();

                AccountName = ber.ReadUnicodeString();

                if (HasGLSTicket == 0x04)
                {
                    UInt32 remainingLength = ber.ReadUInt32();
                    GLSTicketDirect = ber.ReadString();
                }

                TimeSpan span = TimeSpan.FromSeconds(timeStarted);
                LocalTimeStarted = new DateTime(1970, 1, 1).Add(span).ToLocalTime();

            }
            catch (EndOfStreamException eos)
            {
                Debug.WriteLine(eos);
            }
            catch (ObjectDisposedException od)
            {
                Debug.WriteLine(od);
            }
            catch (IOException io)
            {
                Debug.WriteLine(io);
            }

            return this;
        }


    }
}
