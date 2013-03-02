using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Protocol.Generic;
using Protocol.Session;

namespace Protocol
{
    public class ProtocolHandler
    {

        /*
         * Possible header values:
         * 
         * 00 01 00 00 synchronize client request session
         * 00 04 00 00 synack server respond upon request
         * 00 08 00 00 ack client respond on respond
         * 01 00 00 06 server account data? 8 byte in front - parse before when [0] is 1, ...
         * 00 00 00 06 sv account 0 byte in front
         * 08 00 00 06 cl/sv 6 byte in front
         * 00 00 40 00 cl confirm only 4 bytes long
         * 08 00 40 02 sv 4byte seq + 6 bytes?
         * 0B 00 00 02 cl 8 bytes + 10 bytes?
         * 0C 00 40 02 sv 4 bytes seq + 
         * 0F 00 40 02 cl 
         * 03 00 40 02 sv 4 bytes seq + 
         * 0C 00 00 06 
         * 00 00 08 02 server change to world server
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 00 20 00 02 : 44 3B 58 00 00 00 00 00 unknown
         * 03 00 00 02 : 41 91 7D E4      6D E3 46 2C 06 D4 20 BB
         *               41 91 7D E4      1D E8 4F ED 06 D3 D2 A0
         *               41 91 7D E3      FD F9 98 20 06 D3 B3 71
         * 
         * 00 10 00 00 : 6E 25 FB 0D 00 00 00 00
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         */

        // Session establishment byte [1]
        public enum SessionInit
        {
            Synchronize = 0x01, // CL: SYN first client packet
            SynchronizeAcknowledgment = 0x04, // SV: SYN-ACK response from server upon request
            Acknowledgment = 0x08, // CL: ACK from client - second client packet
        }

        // InSession args
        public enum Session
        {
            Terminate = 0x80, //CL and SV: [2]
            ConfirmSequence = 0x40, // CL SV [2]
            ReRequestPacketFromCL = 0x10, // 
            ResendMissingPacketToSV = 0x07, //  [3]
            Data = 0x06, // CL SV [3]
            Unknown2 = 0x02, // unknown [3]
            
        }

        public PayloadData getPayloadSessionInit(byte action_1)
        {
            switch (action_1)
            {
                case (byte)SessionInit.Synchronize:
                    return new SessionSetup.Synchronize();

                case (byte)SessionInit.Acknowledgment:
                    return new SessionSetup.Acknowledgment();

                default:
                    throw new Exception("Default: " + action_1);
            }
        }

        public PayloadData getPayloadObject(byte[] headerAction)
        {
            switch (headerAction[2])
            {
                case (byte)Session.Terminate:
                    return new SessionSetup.Session.Terminate();
                case (byte)Session.ConfirmSequence:   

                    if (headerAction[0] == 0 && headerAction[3] == 0)
                    {
                        return new Server.Session.ConfirmSequence();
                    }
                    return null;
                    //else
                    //{
                        //throw new NotImplementedException("Action 3 not there."); ignore til now
                    //}
                case (byte)Session.Data: // needs handling, because packets can get lost. reorder packets!
                    throw new NotImplementedException("Not implemented.");
                case (byte)Session.ResendMissingPacketToSV:
                    throw new NotImplementedException("Not implemented.");  
                default:
                    throw new Exception("Action 3 nothing found.");
            }

        }

    }
}
