using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
namespace DAT_UNPACKER
{
    internal class Program
    {
        private static readonly string[] FormatDescription = new string[]
		{
			"Bad format. Usage: DAT_UNPACKER.exe <file_name> <flags> <file_id>",
			"Flags and file_id are optional parameters. File_id is presented in hex format (example: A0000394). Available flags:",
			"1 - extract wav files, 2 - extract ogg files, 4 - extract jpg files, 8 - extract dds files, 16 - extract hks files, 256 - extract raw files, 512 - extract selected file (must be used with file_id), 1024 - no extracting, just generating file list.",
			"You have to summ flags for extracting several types of files.",
			"Example: you want to extract only wav, ogg and jpg files. You should use flags = 7. By default, flags = 271."
		};
        public static void WriteHeader(StreamWriter writer)
        {
            for (int i = 0; i < 300; i++)
            {
                writer.Write("#");
            }
            writer.WriteLine();
        }
        public static void StartLog(StreamWriter writer, DateTime now)
        {
            Program.WriteHeader(writer);
            writer.WriteLine("DAT_UNPACKER log started {0}", now);
        }
        public static void EndLog(StreamWriter writer)
        {
            writer.WriteLine("DAT_UNPACKER log ended {0}", DateTime.Now);
            Program.WriteHeader(writer);
        }
        public static void WriteInfo(string info, StreamWriter writer, bool console)
        {
            Program.WriteInfo(info, writer, console, true);
        }
        public static void WriteInfo(string info, StreamWriter writer, bool console, bool newLine)
        {
            if (newLine)
            {
                writer.WriteLine(info);
            }
            else
            {
                writer.Write(info);
            }
            if (console)
            {
                if (newLine)
                {
                    Console.WriteLine(info);
                    return;
                }
                Console.Write(info);
            }
        }
        private static void PrintFormat(StreamWriter writer, bool onlyConsole)
        {
            string[] formatDescription = Program.FormatDescription;
            for (int i = 0; i < formatDescription.Length; i++)
            {
                string text = formatDescription[i];
                if (onlyConsole)
                {
                    Console.WriteLine(text);
                }
                else
                {
                    if (writer != null)
                    {
                        Program.WriteInfo(text, writer, true);
                    }
                }
            }
        }
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Program.PrintFormat(null, true);
                return;
            }
            DateTime now = DateTime.Now;
            string text = args[0];
            string text2 = text.Substring(0, text.IndexOf('.'));
            if (!Directory.Exists(string.Format("data\\{0}", text2)))
            {
                Directory.CreateDirectory(string.Format("data\\{0}", text2));
            }
            using (StreamWriter streamWriter = new StreamWriter(string.Format("data\\{0}\\report_{1}_{2}_{3}_{4}_{5}_{6}.txt", new object[]
			{
				text2,
				now.Day,
				now.Month,
				now.Year,
				now.Hour,
				now.Minute,
				now.Second
			}), false))
            {
                Program.StartLog(streamWriter, now);
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                Console.WriteLine("DAT_UNPACKER {0}.{1}.{2} written by Dancing_on_a_rock_hacker (dancingonarockhacker@gmail.com)", version.Major, version.Minor, version.Build);
                if (args.Length < 1)
                {
                    Program.PrintFormat(streamWriter, false);
                    Program.EndLog(streamWriter);
                }
                else
                {
                    if (!File.Exists("zlib1T.dll"))
                    {
                        Program.WriteInfo("Please move DAT_UNPACKER.exe and datexport.dll into your game folder.", streamWriter, true);
                        Program.EndLog(streamWriter);
                    }
                    else
                    {
                        if (!File.Exists(text))
                        {
                            Program.WriteInfo(string.Format("File {0} was not found.", text), streamWriter, true);
                            Program.EndLog(streamWriter);
                        }
                        else
                        {
                            Options options = Options.LoadAllFiles;
                            if (args.Length > 1)
                            {
                                string s = args[1];
                                int num;
                                if (int.TryParse(s, out num))
                                {
                                    options = (Options)num;
                                }
                            }
                            Console.Write(string.Format("Opening file {0}...", text));
                            DatFile datFile = DatFile.OpenExisting(new DatFileInitParams(text, true));
                            Console.WriteLine("done");
                            Console.Write("Loading dictionary...");
                            Dictionary<int, int> dic = FileDictionary.LoadDictionary(text2);
                            Console.WriteLine("done");
                            if ((options & Options.ExtractSelectedFile) != Options.None && args.Length > 2)
                            {
                                string value = args[2];
                                int key = Convert.ToInt32(value, 16);
                                Program.WriteInfo(string.Format("Extracting file {0}...", key.ToString("X2")), streamWriter, true);
                                Options options2 = Options.LoadAllFiles;
                                if ((options & Options.ExtractRawFile) != Options.None)
                                {
                                    options2 |= Options.ExtractRawFile;
                                }
                                try
                                {
                                    datFile.Files[key].Extract(text2, options2, streamWriter, ref dic);
                                    Program.WriteInfo("File was extracted.", streamWriter, true);
                                }
                                catch
                                {
                                    Program.WriteInfo("File was not found.", streamWriter, true);
                                }
                                Program.EndLog(streamWriter);
                            }
                            else
                            {
                                if ((options & Options.GenerateFileList) != Options.None)
                                {
                                    Console.Write("Generating filelist...");
                                    if (!Directory.Exists("filelist"))
                                    {
                                        Directory.CreateDirectory("filelist");
                                    }
                                    using (StreamWriter streamWriter2 = new StreamWriter(string.Format("filelist\\{0}_filelist.txt", text2), false))
                                    {
                                        foreach (Subfile current in datFile.Subfiles)
                                        {
                                            streamWriter2.WriteLine("File {0} with Size = {1} bytes, Compressed: {2}, Version = {3}", new object[]
											{
												current.DataID.ToString("X2"),
												current.Size,
												current.IsCompressed,
												current.Version
											});
                                        }
                                    }
                                    Console.WriteLine("done");
                                    Program.EndLog(streamWriter);
                                }
                                else
                                {
                                    DateTime now2 = DateTime.Now;
                                    Console.Write("Extracting...");
                                    int num2 = 0;
                                    foreach (Subfile current2 in datFile.Subfiles)
                                    {
                                        if (current2.Extract(text2, options, streamWriter, ref dic))
                                        {
                                            num2++;
                                        }
                                    }
                                    DateTime now3 = DateTime.Now;
                                    Console.WriteLine("done");
                                    Program.WriteInfo(string.Format("Total {0} files were extracted from {1} in ~{2} seconds.", num2, text, (int)(now3 - now2).TotalSeconds), streamWriter, true);
                                    Console.Write("Saving dictionary...");
                                    FileDictionary.SaveDictionary(dic, text2);
                                    Console.WriteLine("done");
                                    Program.EndLog(streamWriter);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
