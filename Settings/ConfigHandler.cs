using System;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Settings
{
    /// <summary>
    /// Load and write config.xml files from/into Config-objects
    /// </summary>
    public class ConfigHandler
    {

        /// <summary>
        /// Read the given config.xml file
        /// </summary>
        /// <param name="fileName">Filename to read the config.xml from</param>
        /// <returns>The static pointer Settings.Config.Instance (e.g. only a
        /// single instance of a config.xml can be used withing a server). If
        /// reading the file failes this will be null.</returns>
        public Config readConfig(string fileName)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Config));

                using (FileStream fs = new FileStream(@fileName, FileMode.Open))
                {
                    Config.Instance = (Config) serializer.Deserialize(fs);
                    Config.Instance.init();
                }
            }
            catch (FileNotFoundException fnf)
            {
                Console.Write(fnf.Message);
            }
            catch (InvalidOperationException ioe)
            {
                Console.Write(ioe.Message);
            }
            catch (SerializationException se)
            {
                Console.Write(se.Message);
            }

            return Config.Instance;
        }

        /// <summary>
        /// Write a Config to the given file
        /// Note: we don't want to write a config right now, but keep the code around
        /// </summary>
        /// <param name="fileName">Filename to write the config.xml to</param>
        /// <param name="config">configuration to write to a file</param>
        static void writeConfig(string fileName, Config config)
        {
            try
            {
				XmlSerializer serializer = new XmlSerializer(config.GetType());

                using (FileStream fs = new FileStream(@fileName, FileMode.Create))
                {

                    XmlWriterSettings xws = new XmlWriterSettings();
                    xws.Indent = true;
                    xws.IndentChars = "\t";

                    using (XmlWriter writer = XmlWriter.Create(fs, xws))
                    {
                        serializer.Serialize(writer, config);
                    }
                }
            }
            catch (SerializationException se)
            {
                Console.Write(se.Message);
            }
        }
    }
}