using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Helper
{
	
	public class HelperMethods : IDisposable
	{
		
		readonly RSACryptoServiceProvider rsaCryptoServiceProvider;
		readonly string privateKeyFile = "data" + Path.DirectorySeparatorChar + "private";
		byte[] sessionKey;
		
		// Emedding of resources implemented like mentioned in http://support.microsoft.com/kb/319292
		
		// the client decrypt part
		int[,] jumpTableClient; // jump table with 2 columns (for bit 0 and bit 1)
		readonly string fileNameTableJumpClient = "data" + Path.DirectorySeparatorChar + "table_jump_client";
		List<byte[][]> lookUpListClient; // List with look-ups 4 columns (cipher, length for encoding, encoded as bitarray, "end value")
		readonly string fileNameTableLookUpClient = "data" + Path.DirectorySeparatorChar + "table_lookup_client";
		byte[][] quickLookUpClient;
		//readonly string fileNameTableJumpClientRaw = "data" + Path.DirectorySeparatorChar + "table_jump_client_raw";
		//readonly string fileNameTableLookUpClientRaw = "data" + Path.DirectorySeparatorChar + "table_lookup_client_raw";
		
		// the server decrypt part
		int[,] jumpTableServer; // jump table with 2 columns (for bit 0 and bit 1)
		readonly string fileNameTableJumpServer = "data" + Path.DirectorySeparatorChar + "table_jump_server";
		List<byte[][]> lookUpListServer; // List with look-ups 4 columns (cipher, length for encoding, encoded as bitarray, "end value")
		readonly string fileNameTableLookUpServer = "data" + Path.DirectorySeparatorChar + "table_lookup_server";
		byte[][] quickLookUpServer;
		//readonly string fileNameTableJumpRaw = "data" + Path.DirectorySeparatorChar + "table_jump_server_raw";
		//readonly string fileNameTableLookUpRaw = "data" + Path.DirectorySeparatorChar + "table_lookup_server_raw";
		
		readonly byte[] clear = { 0x0, 0x0, 0x0, 0x0 }; // for not final check
		
		//readonly string fileNameChecksumArray = "data" + Path.DirectorySeparatorChar + "checksums";
		static readonly HelperMethods INSTANCE = new HelperMethods();

		static readonly object LOCK = new object();

		public static HelperMethods Instance
		{
			get
			{
				return INSTANCE;
			}
		}
		
		HelperMethods()
		{
			// Read the private key extracted from the client
			byte[] cspBlob = File.ReadAllBytes(privateKeyFile);

			// Initialize the RSA
			rsaCryptoServiceProvider = new RSACryptoServiceProvider();
			rsaCryptoServiceProvider.ImportCspBlob(cspBlob);

			// for client packets
			jumpTableClient = generateJumpTable(fileNameTableJumpClient);
			quickLookUpClient = new byte[16372][]; // there are 16372 values
			lookUpListClient = generateLookUpTableClient(fileNameTableLookUpClient);
		
			// for server packets
			jumpTableServer = generateJumpTable(fileNameTableJumpServer);
			quickLookUpServer = new byte[16125][]; // there are 16125 values
			lookUpListServer = generateLookUpTableServer(fileNameTableLookUpServer);
			
		}
		
		public int[,] getJumpTableClient()
		{
			return jumpTableClient;
		}
		
		public List<byte[][]> getLookUpListClient()
		{
			return lookUpListClient;
		}
		
		int[,] generateJumpTable(string fileInputName)
		{
			FileStream fsRead = new FileStream(@fileInputName, FileMode.Open, FileAccess.Read);
			
			int[,] jumpTable = new int[fsRead.Length / 8, 2];
			
			byte[] adress0 = new byte[4];
			byte[] adress1 = new byte[4];
			
			for (int i = 0; i < fsRead.Length / 8; i++)
			{
				
				fsRead.Read(adress0, 0, 4);
				fsRead.Read(adress1, 0, 4);
				
				int value0 = BitConverter.ToInt32(adress0, 0);
				int value1 = BitConverter.ToInt32(adress1, 0);
				
				jumpTable[i, 0] = value0;
				jumpTable[i, 1] = value1;
				
			}
			
			//int v1 = jumpTable[16123, 0];
			//int v2 = jumpTable[16123, 0];
			
			return jumpTable;
		}
		
		List<byte[][]> generateLookUpTableClient(string fileInputName)
		{
			FileStream fsRead = new FileStream(@fileInputName, FileMode.Open);
			
			List<byte[][]> tempLookUpList = new List<byte[][]>();

			int length;
			int counter = 0;
			while ((length = fsRead.ReadByte()) != -1)
			{
				byte[] cipher = new byte[length];
				byte[] encryptArray = new byte[4];
				byte[] endValue = new byte[4];
				byte[] encodingLength = new byte[1];

				fsRead.Read(cipher, 0, length);
				fsRead.Read(encodingLength, 0, 1);
				fsRead.Read(encryptArray, 0, 4);
				fsRead.Read(endValue, 0, 4);

				tempLookUpList.Add(new byte[][]
				{
					cipher,
					encodingLength,
					encryptArray,
					endValue
				});
				quickLookUpClient[counter] = cipher;

				counter++;
			}

			return tempLookUpList;
		}

		public int[,] getJumpTableServer()
		{
			return jumpTableServer;
		}

		public List<byte[][]> getLookUpListServer()
		{
			return lookUpListServer;
		}

		public byte[][] getQuickLookUpListArrayServer()
		{
			return quickLookUpServer;
		}

		int[,] generateJumpTable()
		{
			FileStream fsRead = File.OpenRead(fileNameTableJumpServer);
			int[,] jumpTable = new int[fsRead.Length / 8, 2];
			byte[] adress0 = new byte[4];
			byte[] adress1 = new byte[4];
			int num = 0;
			while (num < fsRead.Length / 8L)
			{
				fsRead.Read(adress0, 0, 4);
				fsRead.Read(adress1, 0, 4);
				int value0 = BitConverter.ToInt32(adress0, 0);
				int value1 = BitConverter.ToInt32(adress1, 0);
				jumpTable[num, 0] = value0;
				jumpTable[num, 1] = value1;
				num++;
			}
			//int v1 = jumpTable[16123, 0];
			//int v2 = jumpTable[16123, 0];
			return jumpTable;
		}

		List<byte[][]> generateLookUpTableServer(string fileInputName)
		{
			FileStream fsRead = File.OpenRead(fileInputName);

			List<byte[][]> tempLookUpList = new List<byte[][]>();
			int counter = 0;
			int length;
			while ((length = fsRead.ReadByte()) != -1)
			{
				byte[] cipher = new byte[length];
				byte[] encryptArray = new byte[4];
				byte[] endValue = new byte[4];
				byte[] encodingLength = new byte[1];

				fsRead.Read(cipher, 0, length);
				fsRead.Read(encodingLength, 0, 1);
				fsRead.Read(encryptArray, 0, 4);
				fsRead.Read(endValue, 0, 4);

				tempLookUpList.Add(new byte[][]
				{
					cipher,
					encodingLength,
					encryptArray,
					endValue
				});

				quickLookUpServer[counter] = cipher;

				counter++;
			}

			return tempLookUpList;
		}

		public byte[] extractSessionKeyFrom1stClientPacket(byte[] simpleBlob)
		{
			byte[] encrypted = new byte[128]; // it's always the same length, because of the 1024 bit key
			Buffer.BlockCopy(simpleBlob, simpleBlob.Length - 128, encrypted, 0, 128);
			Array.Reverse(encrypted); // must be
			sessionKey = rsaCryptoServiceProvider.Decrypt(encrypted, false);
			return sessionKey;
		}

		#region RC4 algo taken from the web at http://dotnet-snippets.de/dns/rc4-verschluesselung-SID594.aspx

		public void RC4(ref byte[] encrypted, byte[] sessionKey)
		{

			if (sessionKey == null)
			{
				throw new Exception("Key hasn't been extracted from first client packet!");
			}

			byte[] s = new byte[256];
			byte[] k = new byte[256];
			byte temp;
			int i, j;
			
			for (i = 0; i < 256; i++)
			{
				s[i] = (byte)i;
				k[i] = sessionKey[i % sessionKey.GetLength(0)];
			}
			
			j = 0;
			for (i = 0; i < 256; i++)
			{
				j = (j + s[i] + k[i]) % 256;
				temp = s[i];
				s[i] = s[j];
				s[j] = temp;
			}
			
			i = j = 0;
			for (int x = 0; x < encrypted.GetLength(0); x++)
			{
				i = (i + 1) % 256;
				j = (j + s[i]) % 256;
				temp = s[i];
				s[i] = s[j];
				s[j] = temp;
				int t = (s[i] + s[j]) % 256;
				encrypted[x] ^= s[t];
			}
		}

		public void RC4(ref byte[] encrypted)
		{
			RC4(ref encrypted, sessionKey);
		}

		public byte[] RC4ToBytes(byte[] encrypted)
		{
			return RC4ToBytes(encrypted, sessionKey);
		}

		public byte[] RC4ToBytes(byte[] encrypted, byte[] sessionKey)
		{
			
			if (sessionKey == null)
			{
				throw new Exception("Key hasn't been extracted from first client packet!");
			}
				
			byte[] s = new byte[256];
			byte[] k = new byte[256];
			byte temp;
			int i, j;
			
			for (i = 0; i < 256; i++)
			{
				s[i] = (byte)i;
				k[i] = sessionKey[i % sessionKey.GetLength(0)];
			}
			
			j = 0;
			for (i = 0; i < 256; i++)
			{
				j = (j + s[i] + k[i]) % 256;
				temp = s[i];
				s[i] = s[j];
				s[j] = temp;
			}
			
			i = j = 0;
			for (int x = 0; x < encrypted.GetLength(0); x++)
			{
				i = (i + 1) % 256;
				j = (j + s[i]) % 256;
				temp = s[i];
				s[i] = s[j];
				s[j] = temp;
				int t = (s[i] + s[j]) % 256;
				encrypted[x] ^= s[t];
			}
			
			return encrypted;
		}
		
#endregion
		
		public bool ArraysEqual(byte[] b1, byte[] b2)
		{
			return Enumerable.SequenceEqual(b1, b2);
		}

        public int getIndexFromByte(byte[][] src, byte[] value)
        {
            int index = 0;
            foreach (byte[] b in src)
            {
                if (Enumerable.SequenceEqual(b, value))
                {
                    break;
                }
                index++;
            }

            if (index == src.Length)
            {
                index = -1;
            }
            
            // could happen some times
            if (index == -1)
            {
                Console.Write("Key not found. This doesn't matter, because the algo will take the next possible or the last, if array > than 4 bytes. org. client does the same");
            }

            return index;
        }

        public bool checkForEndValue(int index, bool isClient)
        {
            List<byte[][]> lookUpList;
            if (isClient)
            {
                lookUpList = lookUpListClient;
            }
            else
            {
                lookUpList = lookUpListServer;
            }

            byte[][] temp = lookUpList[index];
            return Enumerable.SequenceEqual(temp[3], clear);
        }

        public void Dispose()
        {
            if (rsaCryptoServiceProvider != null)
            {
                rsaCryptoServiceProvider.Dispose();
            }
        }

        public void writeLog(string folder, string name, byte[] data, int length, bool isClient)
        {
            lock (LOCK)
            {
                FileStream fileStream;
                if (isClient)
                {
                    fileStream = new FileStream(folder + "\\" + name, FileMode.Create);
                }
                else
                {
                    fileStream = new FileStream(folder + "\\" + name, FileMode.Create);
                }
                fileStream.Write(data, 0, length);
                fileStream.Close();
            }
        }
	}
}
