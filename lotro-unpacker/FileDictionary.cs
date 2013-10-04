using System;
using System.Collections.Generic;
using System.IO;
namespace DAT_UNPACKER
{
    internal static class FileDictionary
    {
        public static Dictionary<int, int> LoadDictionary(string name)
        {
            if (!Directory.Exists(string.Format("data\\{0}", name)))
            {
                Directory.CreateDirectory(string.Format("data\\{0}", name));
            }
            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            if (File.Exists(string.Format("data\\{0}\\dictionary.bin", name)))
            {
                using (BinaryReader binaryReader = new BinaryReader(new FileStream(string.Format("data\\{0}\\dictionary.bin", name), FileMode.Open)))
                {
                    while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                    {
                        int key = binaryReader.ReadInt32();
                        int value = binaryReader.ReadInt32();
                        if (!dictionary.ContainsKey(key))
                        {
                            dictionary.Add(key, value);
                        }
                    }
                }
            }
            return dictionary;
        }
        public static void SaveDictionary(Dictionary<int, int> dic, string name)
        {
            if (!Directory.Exists(string.Format("data\\{0}", name)))
            {
                Directory.CreateDirectory(string.Format("data\\{0}", name));
            }
            using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(string.Format("data\\{0}\\dictionary.bin", name), FileMode.Create)))
            {
                foreach (KeyValuePair<int, int> current in dic)
                {
                    binaryWriter.Write(current.Key);
                    binaryWriter.Write(current.Value);
                }
            }
        }
    }
}
