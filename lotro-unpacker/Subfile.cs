using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
namespace DAT_UNPACKER
{
    public class Subfile : IDisposable
    {
        private IntPtr _dataPtr = IntPtr.Zero;
        private readonly int _did = -1;
        private bool _disposed;
        private readonly int _handle = -1;
        private readonly bool _isCompressed;
        private bool _isDataLoaded;
        private readonly int _iteration = -1;
        private readonly int _size = -1;
        private readonly int _version = -1;
        public IntPtr Data
        {
            get
            {
                if (!this._isDataLoaded)
                {
                    this._dataPtr = Marshal.AllocHGlobal(this._size);
                    int num;
                    DatExport.GetSubfileData(this._handle, this._did, this._dataPtr, 0, out num);
                    this._isDataLoaded = true;
                }
                return this._dataPtr;
            }
        }
        public int DataID
        {
            get
            {
                return this._did;
            }
        }
        internal int Handle
        {
            get
            {
                return this._handle;
            }
        }
        public bool IsCompressed
        {
            get
            {
                return this._isCompressed;
            }
        }
        public int Iteration
        {
            get
            {
                return this._iteration;
            }
        }
        public int Size
        {
            get
            {
                return this._size;
            }
        }
        public int Version
        {
            get
            {
                return this._version;
            }
        }
        internal Subfile(int handle, int did, int size, int iteration, int version)
        {
            this._handle = handle;
            this._did = did;
            this._size = size;
            this._iteration = iteration;
            this._version = version;
            this._isCompressed = (1 == DatExport.GetSubfileCompressionFlag(this._handle, this._did));
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing && !IntPtr.Zero.Equals(this._dataPtr))
                {
                    Marshal.FreeHGlobal(this._dataPtr);
                    this._dataPtr = IntPtr.Zero;
                }
                this._disposed = true;
            }
        }
        public string GetExtension(byte[] buffer, string category, out int headerLength, out byte[] newHeader)
        {
            headerLength = 0;
            newHeader = new byte[0];
            if (category == "0A0")
            {
                headerLength = 8;
                byte b = buffer[headerLength];
                byte b2 = buffer[headerLength + 1];
                byte b3 = buffer[headerLength + 2];
                byte b4 = buffer[headerLength + 3];
                if (b == 82 && b2 == 73 && b3 == 70 && b4 == 70)
                {
                    return "wav";
                }
                if (b == 79 && b2 == 103 && b3 == 103 && b4 == 83)
                {
                    return "ogg";
                }
            }
            else
            {
                if (category == "040")
                {
                    if (buffer.Length >= 12 && buffer[8] == 87 && buffer[9] == 224 && buffer[10] == 224 && buffer[11] == 87)
                    {
                        headerLength = 8;
                        return "hkx";
                    }
                }
                else
                {
                    if (category == "050")
                    {
                        if (buffer.Length >= 21 && buffer[17] == 87 && buffer[18] == 224 && buffer[19] == 224 && buffer[20] == 87)
                        {
                            headerLength = 17;
                            return "hkx";
                        }
                    }
                    else
                    {
                        if (category == "410" || category == "411")
                        {
                            if (buffer.Length >= 28 && buffer[24] == 255 && buffer[25] == 216 && buffer[26] == 255)
                            {
                                headerLength = 24;
                                return "jpg";
                            }
                            if (buffer.Length < 20 || buffer[16] != 68 || buffer[17] != 88 || buffer[18] != 84)
                            {
                                headerLength = 24;
                                newHeader = new byte[128];
                                newHeader[0] = 68;
                                newHeader[1] = 68;
                                newHeader[2] = 83;
                                newHeader[3] = 32;
                                newHeader[4] = 124;
                                newHeader[8] = 7;
                                newHeader[9] = 16;
                                newHeader[12] = buffer[8];
                                newHeader[13] = buffer[9];
                                newHeader[14] = buffer[10];
                                newHeader[15] = buffer[11];
                                newHeader[16] = buffer[12];
                                newHeader[17] = buffer[13];
                                newHeader[18] = buffer[14];
                                newHeader[19] = buffer[15];
                                newHeader[76] = 32;
                                newHeader[80] = 64;
                                newHeader[88] = 24;
                                newHeader[94] = 255;
                                newHeader[97] = 255;
                                newHeader[100] = 255;
                                return "dds";
                            }
                            if (buffer[19] == 49)
                            {
                                newHeader = new byte[128];
                                newHeader[0] = 68;
                                newHeader[1] = 68;
                                newHeader[2] = 83;
                                newHeader[3] = 32;
                                newHeader[4] = 124;
                                newHeader[8] = 7;
                                newHeader[9] = 16;
                                newHeader[12] = buffer[12];
                                newHeader[13] = buffer[13];
                                newHeader[14] = buffer[14];
                                newHeader[15] = buffer[15];
                                newHeader[16] = buffer[8];
                                newHeader[17] = buffer[9];
                                newHeader[18] = buffer[10];
                                newHeader[19] = buffer[11];
                                newHeader[76] = 32;
                                newHeader[80] = 4;
                                newHeader[84] = 68;
                                newHeader[85] = 88;
                                newHeader[86] = 84;
                                newHeader[87] = 49;
                                headerLength = 16;
                                return "dds";
                            }
                            if (buffer[19] == 51)
                            {
                                newHeader = new byte[128];
                                newHeader[0] = 68;
                                newHeader[1] = 68;
                                newHeader[2] = 83;
                                newHeader[3] = 32;
                                newHeader[4] = 124;
                                newHeader[8] = 7;
                                newHeader[9] = 16;
                                newHeader[12] = buffer[12];
                                newHeader[13] = buffer[13];
                                newHeader[14] = buffer[14];
                                newHeader[15] = buffer[15];
                                newHeader[16] = buffer[8];
                                newHeader[17] = buffer[9];
                                newHeader[18] = buffer[10];
                                newHeader[19] = buffer[11];
                                newHeader[22] = 1;
                                newHeader[76] = 32;
                                newHeader[80] = 4;
                                newHeader[84] = 68;
                                newHeader[85] = 88;
                                newHeader[86] = 84;
                                newHeader[87] = 51;
                                newHeader[108] = 8;
                                newHeader[109] = 16;
                                newHeader[110] = 64;
                                headerLength = 24;
                                return "dds";
                            }
                            if (buffer[19] == 53)
                            {
                                newHeader = new byte[128];
                                newHeader[0] = 68;
                                newHeader[1] = 68;
                                newHeader[2] = 83;
                                newHeader[3] = 32;
                                newHeader[4] = 124;
                                newHeader[8] = 7;
                                newHeader[9] = 16;
                                newHeader[10] = 8;
                                newHeader[12] = buffer[12];
                                newHeader[13] = buffer[13];
                                newHeader[14] = buffer[14];
                                newHeader[15] = buffer[15];
                                newHeader[16] = buffer[8];
                                newHeader[17] = buffer[9];
                                newHeader[18] = buffer[10];
                                newHeader[19] = buffer[11];
                                newHeader[22] = 1;
                                newHeader[28] = 1;
                                newHeader[76] = 32;
                                newHeader[80] = 4;
                                newHeader[84] = 68;
                                newHeader[85] = 88;
                                newHeader[86] = 84;
                                newHeader[87] = 53;
                                newHeader[88] = 32;
                                newHeader[94] = 255;
                                newHeader[97] = 255;
                                newHeader[100] = 255;
                                newHeader[107] = 255;
                                newHeader[109] = 16;
                                headerLength = 24;
                                return "dds";
                            }
                        }
                    }
                }
            }
            return "bin";
        }
        public unsafe bool Extract(string name, Options options, StreamWriter writer, ref Dictionary<int, int> dic)
		{
			byte[] array = new byte[this.Size];
			int dataID = this.DataID;
			int size = this.Size;
			int version = this.Version;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = *(byte*)((void*)this.Data + (IntPtr)i / sizeof(void));
			}
			Marshal.FreeHGlobal(this.Data);
			if (this.IsCompressed)
			{
				int num = (int)array[3] << 24 | (int)array[2] << 16 | (int)array[1] << 8 | (int)array[0];
				int num2 = size - 4;
				byte[] array2 = new byte[num];
				byte[] array3 = new byte[num2];
				for (int j = 4; j < size; j++)
				{
					array3[j - 4] = array[j];
				}
				DatExport.uncompress(array2, ref num, array3, num2);
				array = array2;
			}
			string text = dataID.ToString("X8");
			string text2 = text.Substring(0, 1);
			string text3 = text.Substring(1, 1);
			string text4 = text.Substring(2, 1);
			string category = text2 + text3 + text4;
			if (!Directory.Exists(string.Format("data\\{0}\\{1}XXXXXXX", name, text2)))
			{
				Directory.CreateDirectory(string.Format("data\\{0}\\{1}XXXXXXX", name, text2));
			}
			if (!Directory.Exists(string.Format("data\\{0}\\{1}XXXXXXX\\{1}{2}XXXXXX", name, text2, text3)))
			{
				Directory.CreateDirectory(string.Format("data\\{0}\\{1}XXXXXXX\\{1}{2}XXXXXX", name, text2, text3));
			}
			if (!Directory.Exists(string.Format("data\\{0}\\{1}XXXXXXX\\{1}{2}XXXXXX\\{1}{2}{3}XXXXX", new object[]
			{
				name,
				text2,
				text3,
				text4
			})))
			{
				Directory.CreateDirectory(string.Format("data\\{0}\\{1}XXXXXXX\\{1}{2}XXXXXX\\{1}{2}{3}XXXXX", new object[]
				{
					name,
					text2,
					text3,
					text4
				}));
			}
			byte[] array4 = new byte[0];
			int num3 = 0;
			string text5 = ((options & Options.ExtractRawFile) == Options.None) ? this.GetExtension(array, category, out num3, out array4) : "bin";
			string path = string.Format("data\\{0}\\{1}XXXXXXX\\{1}{2}XXXXXX\\{1}{2}{3}XXXXX\\{4}.{5}", new object[]
			{
				name,
				text2,
				text3,
				text4,
				text,
				text5
			});
			bool flag = true;
			string a;
			if ((a = text5) != null)
			{
				if (!(a == "wav"))
				{
					if (!(a == "ogg"))
					{
						if (!(a == "jpg"))
						{
							if (!(a == "dds"))
							{
								if (!(a == "hks"))
								{
									if (a == "bin")
									{
										if ((options & Options.LoadBin) == Options.None)
										{
											flag = false;
										}
									}
								}
								else
								{
									if ((options & Options.LoadHks) == Options.None)
									{
										flag = false;
									}
								}
							}
							else
							{
								if ((options & Options.LoadDds) == Options.None)
								{
									flag = false;
								}
							}
						}
						else
						{
							if ((options & Options.LoadJpg) == Options.None)
							{
								flag = false;
							}
						}
					}
					else
					{
						if ((options & Options.LoadOgg) == Options.None)
						{
							flag = false;
						}
					}
				}
				else
				{
					if ((options & Options.LoadWav) == Options.None)
					{
						flag = false;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
			int num4;
			if (dic.TryGetValue(dataID, out num4) && num4 == version && File.Exists(path))
			{
				return false;
			}
			using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(path, FileMode.Create)))
			{
				if (array4.Length > 0)
				{
					binaryWriter.Write(array4);
				}
				for (int k = 0; k < array.Length; k++)
				{
					if (k >= num3)
					{
						binaryWriter.Write(array[k]);
					}
				}
			}
			int num5;
			if (dic.TryGetValue(dataID, out num5))
			{
				Program.WriteInfo(string.Format("{0}.{1} was extracted. File was modified.", text, text5), writer, false);
				dic[dataID] = version;
			}
			else
			{
				Program.WriteInfo(string.Format("{0}.{1} was extracted. File was added.", text, text5), writer, false);
				dic.Add(dataID, version);
			}
			return true;
		}
        ~Subfile()
        {
            this.Dispose(false);
        }
    }
}
