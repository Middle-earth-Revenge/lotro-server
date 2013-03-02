using System;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

namespace Settings
{
    public class ConfigHandler
    {

        public Settings.Config readConfig(string fileName)
        {
            XmlSerializer serializer = null;

            try
            {
                serializer = new XmlSerializer(typeof(Settings.Config));

                using (FileStream fs = new FileStream(@fileName, FileMode.Open))
                {
                    Settings.Config.Instance = (Settings.Config)serializer.Deserialize(fs);

                    Settings.Config.Instance.init();
                    
                    fs.Close();
                    fs.Dispose();
                }

                serializer = null;
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

        // don't want to write a config
        private void writeConfig(Settings.Config config)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings.Config));

            FileStream fs = new FileStream(@"config.xml", FileMode.Create);

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            xws.IndentChars = "\t";
            
            XmlWriter writer = XmlWriter.Create(fs,xws);

            serializer.Serialize(writer, config);
            
            writer.Close();
            fs.Close();
            fs = null;
            writer = null;          

            serializer = null;
        }
    }
}
