using System;
using System.Xml.Serialization;

namespace AccountControl
{
    /// <summary>
    /// A single character to be used in the game
    /// </summary>
    [Serializable]
    public class Character
    {
        /// <summary>
        /// The name of the character of an account
        /// </summary>
        [XmlElement("CharacterName")]
        public string Name { get; set; }
    }
}
