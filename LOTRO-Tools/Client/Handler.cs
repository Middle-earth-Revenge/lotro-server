using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Concurrent;
using AccountControl;


namespace SessionControl
{
    public sealed class Handler
    {
        private static volatile Handler instance;
        private static object threadLock = new Object();

        private ConcurrentDictionary<UInt16, Session> sessionList = new ConcurrentDictionary<UInt16, Session>(); // Thread safe
        private UInt16 maxConnectedClients = Settings.Config.Instance.MaxClients;

        public static Handler Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (threadLock)
                    {
                        if (instance == null)
                        {
                            instance = new Handler();
                        }
                    }
                }

                return instance;
            }

        }

        // Create a dictionary with max. empty session objects
        // In this case only 10 clients are allowed to be connected simultaniously
        private Handler()
        {
            Session sessionObject = null;

            for (UInt16 i = 1; i <= maxConnectedClients; i++)
                sessionList.TryAdd(i, sessionObject);

        }

        public Session getSession(UInt16 sessionID, EndPoint requestEndpoint)
        {
            Session session = null;

            if (sessionList.ContainsKey(sessionID))
            {
                Session foundSession = sessionList[sessionID];

                if (foundSession != null)
                {

                    if (requestEndpoint.Equals(foundSession.Endpoint))
                    {
                        session = foundSession;
                    }
                }
            }

            return session;
        }

        public bool isSessionSetupComplete(UInt16 sessionID, EndPoint endpoint)
        {
            Session session = getSession(sessionID, endpoint);

            if (session != null)
            {
                return session.SetupComplete;
            }

            return false;
        }

        public bool setSessionSetupComplete(UInt16 sessionID, EndPoint endpoint)
        {
            Session session = getSession(sessionID, endpoint);

            if (session != null)
            {
                session.SetupComplete = true;
                return true;
            }

            return false;
        }

        // Adds session object to session list
        public Session addSession(EndPoint endpoint, string accountName, string clientVersion, DateTime requestLocalStartDate, string GLSTicketDirect)
        {
            Session session = null;

            // check if client session already exists
            // This happens for example after a internet reconnection
            session = getSession(accountName);

            if (session != null) // The user is already logged in and is in session
            {
                    // remove old session, so client will disconnect after a time. otherwise send a terminate.
                    session = null;
                    Debug.WriteLine("Duplicate client session removed.");
            }         

            // "Generate" an new id for client session
            // Look up free "slot"
            UInt16 freeSessionID = getFreeSessionID();

            if (freeSessionID != 0)
            {
                AccountControl.Handler accountHandler = new AccountControl.Handler();

                User userObject = accountHandler.getUser(accountName);

                if(userObject == null)
                 userObject = new User(accountName);

                userObject.IPAddress = endpoint.ToString().Split(':')[0];
                userObject.FoundGLSTicketDirect = GLSTicketDirect;

                session = new Session(userObject);

                session.ID = freeSessionID;
                session.Endpoint = endpoint;
                session.ClientVersion = clientVersion;
                session.LocalTimeStarted = requestLocalStartDate;
                session.SequenceNumberClient = 0x00000000;
                session.SequenceNumberServer = 0x00000000;
                session.StartupKey = 0x0909090907070707; // Generate a random startup session key
                session.ACKNRServer = 0xFFDCE58E; // normally generate a random ACKNR - DC increases in this case FF DC E5 8E 0xD0, 0xDF, 0xDC, 0xFF 
                session.ACKNRClient = 0x0000E58E; // Client responds with this int32, second byte increases FF DC E5 8E 0x00, 0x00, 0xDF, 0xD0 

                sessionList[freeSessionID] = session;
            }
            else
            {
                //throw new Exception("No more free client slots left. Wait til some client disconnects!"); // there must be a response for the client some day
            }

            return session;
        }

        public bool removeSession(UInt16 sessionID, EndPoint endpoint)
        {
            bool success = false;

            Session foundSession = getSession(sessionID, endpoint);

            if (foundSession != null)
            {
                    sessionList[sessionID] = null; // null client object
                    success = true;
             }
             else
             {
                 throw new Exception("Error. Tried to remove non existing client (id: #" + sessionID + ", ip: " + endpoint + ")");
             }
            

            return success;
        }

        // Return: Session object from account name
        private Session getSession(string accountName)
        {
            Session foundSession = null;

            foreach (var session in sessionList)
            {
                foundSession = (Session)session.Value;

                if (foundSession != null)
                {
                    if (foundSession.UserObject.AccountName.Equals(accountName))
                    {
                        return foundSession;
                    }
                }
            }

            return null;
        }

        // Look for a "free slot"
        // Return: free Session ID
        private UInt16 getFreeSessionID()
        {
            foreach (var session in sessionList)
            {
                if (session.Value == null)
                {
                    return session.Key;
                }
            }

            return 0; // failed to find a "free slot"
        }

    }
}
