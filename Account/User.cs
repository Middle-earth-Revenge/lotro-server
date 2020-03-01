using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AccountControl
{
    /// <summary>
    /// An accounts used to login against the server. Consists of many Characters
    /// </summary>
    [Serializable]
    public class User
    {
        public User(string accountName)
        {
            this.AccountName = accountName;
        }

        public User()
        {

        }

        /// <summary>
        /// The name of the account
        /// </summary>
        [XmlElement("AccountName")]
        public string AccountName { get; set; }

        // not needed for this stage of private server version
        [XmlElement("GLSTicketDirect")]
        public string GLSTicketDirect { get; set; }

        public string FoundGLSTicketDirect { get; set; }

        [XmlElement("IPAddress")]
        public string IPAddress { get; set; }

        /// <summary>
        /// Unused at this early stage of private server version
        /// </summary>
        [XmlElement("ServerName")]
        public string ServerName { get; set; }

        [XmlIgnore]
        public SortedList<string, Character> Characters { get; set; }
    }
}
