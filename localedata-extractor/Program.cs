using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace LOTROLOCALDATA
{
    internal static class Program
    {
        private static void Main()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("LOCALDATAEXTRACTOR {0}.{1} written by Dancing_on_a_rock_hacker (dancingonarockhacker@gmail.com)", version.Major, version.Minor);
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());
            if (files.Length == 0)
            {
                Console.WriteLine("No files were found. Please move this programme into the folder with localization .bin files!");
                return;
            }
            Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
            DateTime now = DateTime.Now;
            int num = 0;
            Console.Write("Loading data...");
            string[] array = files;
            for (int i = 0; i < array.Length; i++)
            {
                string text = array[i];
                if (text.EndsWith(".bin"))
                {
                    using (FileStream fileStream = new FileStream(text, FileMode.Open))
                    {
                        using (BinaryReader binaryReader = new BinaryReader(fileStream))
                        {
                            List<string> list = new List<string>();
                            int num2 = binaryReader.ReadInt32();
                            binaryReader.ReadInt32();
                            binaryReader.ReadByte();
                            short num3 = (short)binaryReader.ReadByte();
                            if ((num3 & 128) != 0)
                            {
                                num3 = (short)((int)(num3 ^ 128) << 8 | (int)binaryReader.ReadByte());
                            }
                            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                            {
                                binaryReader.ReadInt64();
                                int num4 = binaryReader.ReadInt32();
                                for (int j = 0; j < num4; j++)
                                {
                                    short num5 = (short)binaryReader.ReadByte();
                                    if ((num5 & 128) != 0)
                                    {
                                        num5 = (short)((int)(num5 ^ 128) << 8 | (int)binaryReader.ReadByte());
                                    }
                                    byte[] bytes = binaryReader.ReadBytes((int)(num5 * 2));
                                    string @string = Encoding.Unicode.GetString(bytes);
                                    list.Add(string.Format("{0}", @string));
                                    num++;
                                }
                                int num6 = binaryReader.ReadInt32();
                                for (int k = 0; k < num6; k++)
                                {
                                    binaryReader.ReadInt32();
                                }
                                byte b = binaryReader.ReadByte();
                                for (int l = 0; l < (int)b; l++)
                                {
                                    int num7 = binaryReader.ReadInt32();
                                    for (int m = 0; m < num7; m++)
                                    {
                                        short num8 = (short)binaryReader.ReadByte();
                                        if ((num8 & 128) != 0)
                                        {
                                            num8 = (short)((int)(num8 ^ 128) << 8 | (int)binaryReader.ReadByte());
                                        }
                                        byte[] bytes2 = binaryReader.ReadBytes((int)(num8 * 2));
                                        string string2 = Encoding.Unicode.GetString(bytes2);
                                        list.Add(string.Format("{0}", string2));
                                        num++;
                                    }
                                }
                            }
                            if (!dictionary.ContainsKey(num2.ToString("X2")))
                            {
                                dictionary.Add(num2.ToString("X2"), list);
                            }
                        }
                    }
                }
            }
            Console.WriteLine("done");
            Console.Write("Saving texts...");
            using (StreamWriter streamWriter = new StreamWriter("LocalData.txt"))
            {
                foreach (KeyValuePair<string, List<string>> current in dictionary)
                {
                    for (int n = 0; n < current.Value.Count; n++)
                    {
                        streamWriter.WriteLine("{0} - {1}", current.Key + n.ToString("X2"), current.Value[n]);
                    }
                }
            }
            DateTime now2 = DateTime.Now;
            Console.WriteLine("done");
            Console.WriteLine("{0} texts were extracted in {1} seconds.", num, (int)(now2 - now).TotalSeconds);
        }
    }
}
