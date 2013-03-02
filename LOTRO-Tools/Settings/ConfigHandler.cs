using System;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
        public Settings.Config readConfig(string fileName)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings.Config));

                using (FileStream fs = new FileStream(@fileName, FileMode.Open))
                {
                    Settings.Config.Instance = (Settings.Config) serializer.Deserialize(fs);

                    Settings.Config.Instance.init();
                    
                    fs.Dispose();
                }
            }
            catch (FileNotFoundException fnf)
            {
                Console.Write(fnf.Message);
            }
            catch (System.InvalidOperationException ioe)
            {
                Console.Write(ioe.Message);
            }
            catch (SerializationException se)
            {
                Console.Write(se.Message);
            }

            return Settings.Config.Instance;
        }

        /// <summary>
        /// Write a Config to the given file
        /// Note: we don't want to write a config right now, but keep the code around
        /// </summary>
        /// <param name="fileName">Filename to write the config.xml to</param>
        /// <param name="config">configuration to write to a file</param>
        private void writeConfig(string fileName, Settings.Config config)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings.Config));

                using (FileStream fs = new FileStream(@fileName, FileMode.Create))
                {

                    XmlWriterSettings xws = new XmlWriterSettings();
                    xws.Indent = true;
                    xws.IndentChars = "\t";

                    using (XmlWriter writer = XmlWriter.Create(fs, xws))
                    {
                        serializer.Serialize(writer, config);
                        writer.Close();
                    }

                    fs.Dispose();
                }
            }
            catch (SerializationException se)
            {
                Console.Write(se.Message);
            }
        }
    }
}
