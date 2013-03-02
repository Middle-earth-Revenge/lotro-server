using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Account
{
    [Serializable]
    class Character
    {
        /// <summary>
        /// The name of the character of an account
        /// </summary>
        [XmlElement("CharacterName")]
        public string Name { get; set; }
    }
}
