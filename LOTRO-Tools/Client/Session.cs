using System;
using System.Net;
using AccountControl;

namespace SessionControl
{
    public class Session
    {
        public enum Status
        {
            AccountAlreadyConnected = 0x01, // 
            AccountInvalid = 0x02, //  
            ErrorRequestAccountInfo = 0x03, // 
            RemovedBecauseAlreadyLogin = 0x04, // 
            AccountServerNotReachable = 0x05, //  
            ServerCrashed = 0x06, // 
            AccountServerNotReachable2 = 0x07, // 
            CharacterDroppedFromWorld = 0x08, // 
        }

        public UInt16 ID { get; set; } // Session ID
        public string ClientVersion { get; set; } // e.g. 061004_netver:7249; didver:926CD8E3-2984-4CA9-9C6B-6DF2C8EB6BC3
        public DateTime LocalTimeStarted { get; set; } // since 01.01.1970
        public DateTime lastTimeAccessed { get; set; } // A worker thread must parse the session list for timed out (inactive) sessions to gain free id space

        public User UserObject { get; private set; }
        public EndPoint Endpoint { get; set; }

        public UInt64 StartupKey { get; set; }
        public bool SetupComplete { get; set; }

        public UInt32 SequenceNumberClient { get; set; } // 
        public UInt32 SequenceNumberServer { get; set; } // 
        public UInt32 ACKNRServer { get; set; }
        public UInt32 ACKNRClient { get; set; }

        public UInt32 InitialChecksumServer { get; private set; }
        public UInt32 InitialChecksumClient { get; private set; }

        Helper.Checksum checksumServer = null;
        Helper.Checksum checksumClient = null;
        private UInt32[] chksumTableServer = new UInt32[256];
        private UInt32[] chksumTableClient = new UInt32[256];

        // to be implemented:
        // last ten received packets
        // last ten send to packets
        // queue for merging packets which are fragmented

        public Session(User userObject)
        {
            this.UserObject = userObject;

            InitialChecksumServer = 0x97196156; // randomize in real when a new client session should established
            InitialChecksumClient = 0x9AF7CFD2; // randomize in real when a new client session should established

            checksumServer = new Helper.Checksum();
            chksumTableServer = checksumServer.generateInitialChecksumTable(InitialChecksumServer);

            checksumClient = new Helper.Checksum();
            chksumTableClient = checksumClient.generateInitialChecksumTable(InitialChecksumClient); 

        }

        // neccessary when in session and sending a 06 packet
        // 
        public UInt32 getServerPayloadChecksumXOR(UInt32 sequenceNr)
        {
            sequenceNr -= 2;

            UInt32 value = sequenceNr % 256;

            // After 255 sequence nr's a new checksum table will be calculated
            // Starts with seq nr 0x00000002
            if (sequenceNr > 255 && sequenceNr % 256 == 0) 
            {
                chksumTableServer = checksumServer.updateChecksumTable();
            }

            return chksumTableServer[255 - (value)];
        }

        // neccessary to validate client packet checksum
        public UInt32 getClientPayloadChecksumXOR(UInt32 sequenceNr)
        {
            sequenceNr -= 2;

            UInt32 value = sequenceNr % 256;

            // After 255 sequence nr's a new checksum table will be calculated
            // Starts with seq nr 0x00000002
            if (sequenceNr > 255 && sequenceNr % 256 == 0)
            {
                chksumTableClient = checksumClient.updateChecksumTable();
            }

            return chksumTableClient[255 - (value)];
        }

    }
}
