using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Account
{
    /// <summary>
    /// A single character to be used in the game
    /// </summary>
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
