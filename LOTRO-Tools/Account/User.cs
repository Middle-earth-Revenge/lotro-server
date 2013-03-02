using System;
using System.Text;
using System.Xml.Serialization;

namespace Account
{
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

        /// <summary>
        /// Unused at this early stage of private server version
        /// </summary>
        [XmlElement("Password")]
        public string Password { get; set; }

        [XmlElement("IPAddress")]
        public string IPAddress { get; set; }

        /// <summary>
        /// Unused at this early stage of private server version
        /// </summary>
        [XmlElement("ServerName")]
        public string ServerName { get; set; }

        /// <summary>
        /// Unused at this early stage of private server version
        /// </summary>
        [XmlElement("SessionTicket")]
        public string SessionTicket { get; set; }

    }
}
