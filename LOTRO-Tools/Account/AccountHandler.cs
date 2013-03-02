using System;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Account
{
    public class AccountHandler
    {
        private List<User> validAccounts = new List<User>();

        public AccountHandler()
        {
            // in reality a account has to be authenticated through the webservice and then added to the list
            // in this case we read "valid" accounts from a folder 

        }

        public User getUser(string accountName)
        {
            User user = parseUser(accountName);

            return user;
        }

        private User parseUser(string accountName)
        {
            XmlSerializer serializer = null;
            User user = null;

            try
            {
                serializer = new XmlSerializer(typeof(Account.User));

                using (FileStream fs = new FileStream(@Settings.Config.Instance.AccountFolder + "\\" + accountName + ".xml", FileMode.Open))
                {
                    user = (Account.User)serializer.Deserialize(fs);

                    fs.Close();
                    fs.Dispose();
                }

                serializer = null;
            }
            catch (FileNotFoundException fnf)
            {
                Debug.WriteIf(Settings.Config.Instance.Debug, fnf.Message, DateTime.Now.ToString() + " " + this.GetType().Name + ".GetUser");
                Debug.Flush();
            }
            catch (System.InvalidOperationException ioe)
            {
                Debug.WriteIf(Settings.Config.Instance.Debug, ioe.Message, DateTime.Now.ToString() + " " + this.GetType().Name + ".GetUser");
                Debug.Flush();
            }
            catch (SerializationException se)
            {
                Debug.WriteIf(Settings.Config.Instance.Debug, se.Message, DateTime.Now.ToString() + " " + this.GetType().Name + ".GetUser");
                Debug.Flush();
            }

            return user;
        }
    }
}
