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

        [XmlElement("AccountName")]
        public string AccountName { get; set; }

        // Not needed for this stage of private server version
        [XmlElement("Password")]
        public string Password { get; set; }

        [XmlElement("IPAddress")]
        public string IPAddress { get; set; }

        [XmlElement("ServerName")]
        public string ServerName { get; set; }

        // not needed for this stage of private server version
        [XmlElement("SessionTicket")]
        public string SessionTicket { get; set; }

    }
}
