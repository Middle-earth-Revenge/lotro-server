using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;


namespace Account
{
    public sealed class SessionHandler
    {
        private static volatile SessionHandler instance;
        private static object threadLock = new Object();

        private Dictionary<UInt16, Session> connectedClients = new Dictionary<UInt16, Session>(10);

        public static SessionHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (threadLock)
                    {
                        if (instance == null)
                        {
                            instance = new SessionHandler();
                        }
                    }
                }

                return instance;
            }

        }

        private SessionHandler()
        {
            // Create a dictionary with max. user objects
            // In this case only 10 clients are allowed simultaniously
            Session clientSessionObject = null;

            for (UInt16 i = 1; i <= 1; i++)
                connectedClients.Add(i, clientSessionObject);

        }

        public bool isInSession(UInt16 clientSessionID, EndPoint endpoint)
        {
            bool isValid = false;

            Session clientSession = getClientSession(clientSessionID, endpoint);

            if (clientSession != null)
            {
                isValid = true;
            }

            return isValid;
        }

        public Session getClientSession(UInt16 clientSessionID, EndPoint requestEndpoint)
        {
            Session validClient = null;

            if (connectedClients.ContainsKey(clientSessionID))
            {
                Session foundClientSession = connectedClients[clientSessionID];

                if (foundClientSession != null)
                {
                    if (requestEndpoint.Equals(foundClientSession.Endpoint))
                    {
                        validClient = foundClientSession;
                    }
                }
            }

            return validClient;

        }

        public bool isSessionSetupComplete(UInt16 clientSessionID, EndPoint endpoint)
        {
            bool isCompleted = false;

            Session clientSession = getClientSession(clientSessionID, endpoint);

            if (clientSession != null)
            {
                if (clientSession.SetupComplete)
                    isCompleted = true;
            }

            return isCompleted;
        }

        public bool setSessionSetupComplete(UInt16 clientSessionID, EndPoint endpoint)
        {
            bool isValid = false;

            Session clientSession = getClientSession(clientSessionID, endpoint);

            if (clientSession != null)
            {
                clientSession.SetupComplete = true;
                isValid = true;
            }

            return isValid;
        }

        // Adds client object to client list
        public Session addClientSession(EndPoint endpoint, string accountName, string clientVersion, DateTime requestStartDate)
        {
            Session clientSession = null;

            // check if client session already exists
            clientSession = getClientSession(accountName);

            if (clientSession != null) // The user is already logged in and in session
            {
                    // remove old session, client will disconnect after a time. otherwise send a terminate.
                    clientSession = null;
                    Debug.WriteLine("Duplicated client session removed.");
            }         

            // "Generate" an id for client
            // Look up free "slot"
            UInt16 freeID = getFreeClientID();

            if (freeID != 0)
            {
                AccountHandler accountHandler = new AccountHandler();

                User userObject = new User(accountName);

                userObject.IPAddress = endpoint.ToString().Split(':')[0];

                clientSession = new Session(userObject);

                clientSession.ID = freeID;
                clientSession.Endpoint = endpoint;
                clientSession.ClientVersion = clientVersion;
                clientSession.LocalTimeStarted = requestStartDate;
                clientSession.SequenceNumberClient = 0x00000000;
                clientSession.SequenceNumberServer = 0x00000000;
                clientSession.StartupKey = 0x0909090907070707; // Generate a random startup session key
                clientSession.ACKNRServer = 0xFFDCE58E; // normally generate a random ACKNR - DC increases in this case FF DC E5 8E 0xD0, 0xDF, 0xDC, 0xFF 
                clientSession.ACKNRClient = 0x0000E58E; // Client responds with this int32, second byte increases FF DC E5 8E 0x00, 0x00, 0xDF, 0xD0 
                
                connectedClients[freeID] = clientSession;
            }
            else
            {
                //throw new Exception("No more free client slots left. Wait til some client disconnects!"); // there must be a response for the client some day
            }

            return clientSession;
        }

        public bool removeClientSession(UInt16 clientID, EndPoint endpoint)
        {
            bool success = false;

            Session foundClientSession = getClientSession(clientID, endpoint);

            if (foundClientSession != null)
            {
                    connectedClients[clientID] = null; // null client object

                    success = true;
             }
             else
             {
                 throw new Exception("Error. Tried to remove non existing client (id: #" + clientID + ", ip: " + endpoint + ")");
             }
            

            return success;
        }

        // Return: Client session object from account name
        private Session getClientSession(string accountName)
        {
            Session foundClientSession = null;

            foreach (var clientSession in connectedClients)
            {
                foundClientSession = (Session)clientSession.Value;

                if (foundClientSession != null)
                {
                    if (foundClientSession.UserObject.AccountName.Equals(accountName))
                    {
                        return foundClientSession;
                    }
                }
            }

            return null;
        }

        // Look for a free slot
        // Return: Session ID
        private UInt16 getFreeClientID()
        {
            UInt16 freeSlot = 0;

            foreach (var client in connectedClients)
            {
                if (client.Value == null)
                {
                    return client.Key;
                }
            }

            return freeSlot;
        }

    }
}
