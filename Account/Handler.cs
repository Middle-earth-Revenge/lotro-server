﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace AccountControl
{
    /// <summary>
    /// Loads accounts/characters from file
    /// </summary>
    public class Handler
    {
        private List<User> validAccounts = new List<User>();

        public Handler()
        {
            // in reality a account has to be authenticated through the webservice and then added to the list
            // in this case we read "valid" accounts from a folder 

        }

        /// <summary>
        /// Load a user and its accounts from file
        /// </summary>
        /// <param name="accountName">the name of the authenticated account</param>
        /// <returns>the data read from the file (or null if reading failed)</returns>
        public User getUser(string accountName)
        {
            User user = parseUser(accountName);
// TODO: commented for now, not implemented in tammo's code
//            if (user != null)
//            {
//                parseAccounts(user, accountName);
//            }
            return user;
        }

        /// <summary>
        /// Read the user from its xml file
        /// </summary>
        /// <param name="accountName">the name of the authenticated account</param>
        /// <returns>the data read from the file (or null if reading failed)</returns>
        private User parseUser(string accountName)
        {
            User user = null;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(User));
                using (FileStream fs = new FileStream(@Settings.Config.Instance.AccountFolder + "\\" + accountName + ".xml", FileMode.Open))
                {
                    user = (User) serializer.Deserialize(fs);
                }
            }
            catch (IOException ioe)
            {
                Debug.WriteIf(Settings.Config.Instance.Debug, ioe.Message, DateTime.Now.ToString() + " " + this.GetType().Name + ".parseUser");
                Debug.Flush();
            }
            catch (SystemException se)
            {
                Debug.WriteIf(Settings.Config.Instance.Debug, se.Message, DateTime.Now.ToString() + " " + this.GetType().Name + ".parseUser");
                Debug.Flush();
            }

            return user;
        }

        /// <summary>
        /// Read all characters for a given account
        /// </summary>
        /// <param name="user">logged in user to add the characters to</param>
        /// <param name="accountName">name of the logged in account</param>
        private void parseAccounts(User user, string accountName)
        {
            string[] characterFiles = Directory.GetFiles(@Settings.Config.Instance.AccountFolder + "\\" + accountName);

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Character));
                foreach (string characterFile in characterFiles)
                {
                    using (FileStream fs = new FileStream(characterFile, FileMode.Open))
                    {
                        Character character = (Character) serializer.Deserialize(fs);
                        user.Characters.Add(character.Name, character);
                    }
                }
            }
            catch (IOException ioe)
            {
                Debug.WriteIf(Settings.Config.Instance.Debug, ioe.Message, DateTime.Now.ToString() + " " + this.GetType().Name + ".parseAccounts");
                Debug.Flush();
            }
            catch (SystemException se)
            {
                Debug.WriteIf(Settings.Config.Instance.Debug, se.Message, DateTime.Now.ToString() + " " + this.GetType().Name + ".parseAccounts");
                Debug.Flush();
            }

        }
    }
}
