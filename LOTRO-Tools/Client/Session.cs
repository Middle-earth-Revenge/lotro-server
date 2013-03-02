using System;
using System.Net;

namespace Account
{
    public class Session
    {
        public UInt16 ID { get; set; } // Session ID
        public string ClientVersion { get; set; } // e.g. 061004_netver:7249; didver:926CD8E3-2984-4CA9-9C6B-6DF2C8EB6BC3
        public DateTime LocalTimeStarted { get; set; } // in payload data since 01.01.1970

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


        // last ten received packets
        // last ten send to packets
        // queue for merging packets which a fragmented

        public Session(User userObject)
        {
            this.UserObject = userObject;

            InitialChecksumServer = 0x97196156; // randomize in real
            InitialChecksumClient = 0x9AF7CFD2; // randomize in real

            checksumServer = new Helper.Checksum();
            chksumTableServer = checksumServer.generateInitialChecksumTable(InitialChecksumServer);

            checksumClient = new Helper.Checksum();
            chksumTableClient = checksumClient.generateInitialChecksumTable(InitialChecksumClient); 

        }

        // neccessary when in session
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
