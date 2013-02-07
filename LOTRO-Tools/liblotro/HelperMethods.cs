using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections;

namespace LOTRO
{

	class HelperMethods
	{

		private RSACryptoServiceProvider rsaCryptoServiceProvider;
		private readonly string privateKeyFile = "data\\private";
		private byte[] cspBlob;
		private byte[] sessionKey;

		// the client decrypt part
		private int[,] jumpTableClient; // jump table with 2 columns (for bit 0 and bit 1)
		private readonly string fileNameTableJumpClient = "data\\table_jump_client";
		private List<byte[][]> lookUpListClient; // List with look-ups 4 columns (cipher, length for encoding, encoded as bitarray, "end value")
		private readonly string fileNameTableLookUpClient = "data\\table_lookup_client";
		private byte[][] quickLookUpClient;

		// the server decrypt part
		private int[,] jumpTableServer; // jump table with 2 columns (for bit 0 and bit 1)
		private readonly string fileNameTableJumpServer = "data\\table_jump_server";
		private List<byte[][]> lookUpListServer; // List with look-ups 4 columns (cipher, length for encoding, encoded as bitarray, "end value")
		private readonly string fileNameTableLookUpServer = "data\\table_lookup_server";
		private byte[][] quickLookUpServer;

		private readonly byte[] clear = { 0x0, 0x0, 0x0, 0x0 }; // for not final check

		private int[] checksums;
		private readonly string fileNameChecksumArray = "data\\checksums";

		private static HelperMethods instance;

		public static HelperMethods Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new HelperMethods();
				}
				return instance;
			}
		}

		private HelperMethods()
		{
			rsaCryptoServiceProvider = new RSACryptoServiceProvider();

			FileStream fsInput = new FileStream(@privateKeyFile, FileMode.Open);

			cspBlob = new byte[fsInput.Length];

			fsInput.Read(cspBlob, 0, cspBlob.Length);

			rsaCryptoServiceProvider.ImportCspBlob(cspBlob);

			fsInput.Close();

			// for client packets
			this.jumpTableClient = generateJumpTableClient(fileNameTableJumpClient);
			this.quickLookUpClient = new byte[16372][]; // there are 16371 values
			this.lookUpListClient = generateLookUpTableClient(fileNameTableLookUpClient);

			// for server packets
			this.jumpTableServer = generateJumpTableServer(fileNameTableJumpServer);
			this.quickLookUpServer = new byte[16125][]; // there are 16125 values
			this.lookUpListServer = generateLookUpTableServer(fileNameTableLookUpServer);

			// for checksums, not complete til now
			this.checksums = generateChecksumArray(fileNameChecksumArray);
		}

		// reads out the checksum values from file
		private int[] generateChecksumArray(string fileInputName)
		{
			FileStream fsRead = new FileStream(@fileInputName, FileMode.Open);

			int[] tempArray = new int[fsRead.Length / 4 + 1];
			byte[] entry = new byte[4];

			int counter = 0;

			while (fsRead.Read(entry,0,4) != 0)
			{
				Buffer.BlockCopy(entry, 0, tempArray, counter*4, 4);

				counter++;
			}



			return tempArray;
		}

		public int[,] getJumpTableClient()
		{
			return this.jumpTableClient;
		}

		public List<byte[][]> getLookUpListClient()
		{
			return this.lookUpListClient;
		}

		private int[,] generateJumpTableClient(string fileInputName)
		{
			FileStream fsRead = new FileStream(@fileInputName, FileMode.Open);

			int[,] jumpTable = new Int32[fsRead.Length / 8, 2];

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

		private List<byte[][]> generateLookUpTableClient(string fileInputName)
		{
			FileStream fsRead = new FileStream(@fileInputName, FileMode.Open);

			List<byte[][]> tempLookUpList = new List<byte[][]>();

			byte[][] entry;

			int length = 0;
			int counter = 0;
			byte[] encryptArray;
			byte[] endValue;
			byte[] encodingLength;
			byte[] cipher;

			while ((length = fsRead.ReadByte()) != -1)
			{
				cipher = new byte[length];
				encryptArray = new byte[4];
				endValue = new byte[4];
				encodingLength = new byte[1];

				fsRead.Read(cipher, 0, length);
				fsRead.Read(encodingLength, 0, 1);
				fsRead.Read(encryptArray, 0, 4);
				fsRead.Read(endValue, 0, 4);

				entry = new byte[4][];

				entry[0] = cipher;
				entry[1] = encodingLength;
				entry[2] = encryptArray;
				entry[3] = endValue;

				tempLookUpList.Add(entry);
				quickLookUpClient[counter] = cipher;

				counter++;
			}



			return tempLookUpList;
		}

		public int[,] getJumpTableServer()
		{
			return this.jumpTableServer;
		}

		public List<byte[][]> getLookUpListServer()
		{
			return this.lookUpListServer;
		}

		public byte[][] getQuickLookUpListArrayServer()
		{
			return this.quickLookUpServer;
		}

		private int[,] generateJumpTableServer(string fileInputName)
		{
			FileStream fsRead = new FileStream(@fileInputName, FileMode.Open);

			int[,] jumpTable = new Int32[fsRead.Length / 8, 2];

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

		private List<byte[][]> generateLookUpTableServer(string fileInputName)
		{
			FileStream fsRead = new FileStream(@fileInputName, FileMode.Open);

			List<byte[][]> tempLookUpList = new List<byte[][]>();

			byte[][] entry;

			int length = 0;
			int counter = 0;
			byte[] encryptArray;
			byte[] endValue;
			byte[] encodingLength;
			byte[] cipher;

			while ((length = fsRead.ReadByte()) != -1)
			{
				cipher = new byte[length];
				encryptArray = new byte[4];
				endValue = new byte[4];
				encodingLength = new byte[1];

				fsRead.Read(cipher, 0, length);
				fsRead.Read(encodingLength, 0, 1);
				fsRead.Read(encryptArray, 0, 4);
				fsRead.Read(endValue, 0, 4);

				entry = new byte[4][];

				entry[0] = cipher;
				entry[1] = encodingLength;
				entry[2] = encryptArray;
				entry[3] = endValue;

				tempLookUpList.Add(entry);
				quickLookUpServer[counter] = cipher;

				counter++;
			}



			return tempLookUpList;
		}

		// only works if you inject my generated public key into the client.exe
		public byte[] extractSessionKeyFrom1stClientPacket(byte[] simpleBlob)
		{

			byte[] encrypted = new byte[128]; // it's always the same length, because of the 1024 bit private key

			Buffer.BlockCopy(simpleBlob, simpleBlob.Length - 128, encrypted, 0, 128);

			Array.Reverse(encrypted); // must be

			this.sessionKey = rsaCryptoServiceProvider.Decrypt(encrypted, false);

			return this.sessionKey;
		}

		#region RC4 algo taken from the web at http://dotnet-snippets.de/dns/rc4-verschluesselung-SID594.aspx

		public void RC4(ref Byte[] encrypted, byte[] sessionKey)
		{

			if (sessionKey != null)
			{

				Byte[] s = new Byte[256];
				Byte[] k = new Byte[256];
				Byte temp;
				int i, j;

				for (i = 0; i < 256; i++)
				{
					s[i] = (Byte)i;
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
			else
			{
				throw new Exception("Key hasn't been extracted from first client packet!");
			}

		}

		public void RC4(ref Byte[] encrypted)
		{
			RC4(ref encrypted, this.sessionKey);
		}

		public byte[] RC4ToBytes(Byte[] encrypted)
		{
			return RC4ToBytes(encrypted, this.sessionKey);
		}

		public byte[] RC4ToBytes(Byte[] encrypted, byte[] sessionKey)
		{

			if (sessionKey != null)
			{

				Byte[] s = new Byte[256];
				Byte[] k = new Byte[256];
				Byte temp;
				int i, j;

				for (i = 0; i < 256; i++)
				{
					s[i] = (Byte)i;
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
			else
			{
				throw new Exception("Key hasn't been extracted from first client packet!");
			}

		}

#endregion

		// needs to be replaced, taken from the net
		public bool ArraysEqual(byte[] b1, byte[] b2)
		{
			unsafe
			{
				if (b1.Length != b2.Length)
					return false;

				int n = b1.Length;

				fixed (byte* p1 = b1, p2 = b2)
				{
					byte* ptr1 = p1;
					byte* ptr2 = p2;

					while (n-- > 0)
					{
						if (*ptr1++ != *ptr2++)
							return false;
					}
				}

				return true;
			}
		}

		public int getIndexFromByte(byte[][] src, byte[] value)
		{
			int index = 0;

			foreach (byte[] b in src)
			{
				if (ArraysEqual(b, value))
					break;
				index++;
			}

			if (index == src.Length)
				index = -1;

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
				lookUpList = lookUpListClient;
			else
				lookUpList = lookUpListServer;


			bool containsNoEndValue = false;

			byte[][] temp = lookUpList[index];

			containsNoEndValue = ArraysEqual(temp[3], clear);

			return containsNoEndValue;
		}


		// this isn't working correct till now, because packetnmbrs increasing more than checksum values. i have to figure this out what happens if values are higher :-( sry.
		public byte[] generateChecksumForHeader(byte[] headerModded, int checksumPacket)
		{
			byte[] begin = new byte[] { 0x00, 0x00, 0x14, 0x00 };
			byte[] h1 = new byte[4];// { 0xf2,0x00,0x2c,0x00 };//          0x00, 0x2c, 0x00,0xf2};
			byte[] h2 = new byte[4];// { 0x00,0x00,0x04,0x00};
			byte[] h3 = new byte[4];
			byte[] constantBytes = new byte[] { 0xdd, 0x70, 0xdd, 0xba };
			byte[] h4 = new byte[4];// { 0xDC,0xFF,0xAD,0x61 };

			byte[] h5 = new byte[4];// {0xf5, 0xfb,0x03,0x30};

			//byte[] ori;// = new byte[] { 0xf7, 0x57, 0x70, 0xbf };// hier muss in tabelle nachgeschaut werden und zwar: (1 - paketnr)*4 + edx           (edx = anfang der chk taballe)

			if (headerModded.Length > 0x14)
			{
				System.Diagnostics.Debug.WriteLine("Not a valid header!");
				return null;
			}

			Buffer.BlockCopy(headerModded, 0, h1, 0, 4);
			Buffer.BlockCopy(headerModded, 4, h2, 0, 4);
			Buffer.BlockCopy(headerModded, 8, h3, 0, 4);
			Buffer.BlockCopy(headerModded, 12, h4, 0, 4);
			Buffer.BlockCopy(headerModded, 16, h5, 0, 4); // diesen muss ich wissen um ihn richtig zu generieren


			int start = BitConverter.ToInt32(begin, 0);
			int h1Int = BitConverter.ToInt32(h1, 0);
			int h2Int = BitConverter.ToInt32(h2, 0);
			int h3Int = BitConverter.ToInt32(h3, 0);
			int h4Int = BitConverter.ToInt32(constantBytes, 0);
			int h5Int = BitConverter.ToInt32(h4, 0);
			int h6Int = BitConverter.ToInt32(h5, 0);

			int finA = start + h1Int + h2Int + h3Int + h4Int + h6Int;

			int finB = h5Int - finA;

			int checksumLookup = (256 - h3Int + 1);// *4;

			string hexstring2 = checksums[checksumLookup].ToString("X");

			int checkMod = checksums[checksumLookup];

			System.Diagnostics.Debug.WriteLine(hexstring2); // "9215CB38" 51 10 3B 19


			if (h3Int == 0)
				checkMod = 0;


			int finalChecksum = checkMod ^ finB; // -513519573 final chk die kann ich bekommen

			string hexstring = finalChecksum.ToString("X"); // reverse ist checksum

			System.Diagnostics.Debug.WriteLine(hexstring); // "9215CB38" 51 10 3B 19


			// insert in head

			return null;
		}

		// this isn't working correct till now
		public byte[] getChecksumFromHeader(byte[] header)
		{
			byte[] begin = new byte[] {0x00,0x00,0x14, 0x00};
			byte[] h1 = new byte[4];// { 0xf2,0x00,0x2c,0x00 };//          0x00, 0x2c, 0x00,0xf2};
			byte[] h2 = new byte[4];// { 0x00,0x00,0x04,0x00};
			byte[] h3 = new byte[4];
			byte[] constantBytes = new byte[] { 0xdd,0x70,0xdd,0xba };
			byte[] h4 = new byte[4];// { 0xDC,0xFF,0xAD,0x61 };

			byte[] h5 = new byte[4];// {0xf5, 0xfb,0x03,0x30};

			byte[] ori = new byte[] { 0xf7, 0x57, 0x70, 0xbf };// hier muss in tabelle nachgeschaut werden und zwar: (1 - paketnr)*4 + edx           (edx = anfang der chk taballe)

			// das kann super ätzend werden :-( am besten checksummen check entfernen!!!

			if (header.Length > 0x14)
			{
				System.Diagnostics.Debug.WriteLine("Not a valid header!");
				return null;
			}

			Buffer.BlockCopy(header, 0, h1, 0, 4);
			Buffer.BlockCopy(header, 4, h2, 0, 4);
			Buffer.BlockCopy(header, 8, h3, 0, 4);
			Buffer.BlockCopy(header, 12, h4, 0, 4);
			Buffer.BlockCopy(header, 16, h5, 0, 4);


			int start = BitConverter.ToInt32(begin, 0);
			int h1Int = BitConverter.ToInt32(h1, 0);
			int h2Int = BitConverter.ToInt32(h2, 0);
			int h3Int = BitConverter.ToInt32(h3, 0);
			int h4Int = BitConverter.ToInt32(constantBytes, 0);
			int h5Int = BitConverter.ToInt32(h4, 0);
			int h6Int = BitConverter.ToInt32(h5, 0);

			int finA = start + h1Int + h2Int + h3Int + h4Int + h6Int;

			int finB = h5Int - finA;

			int oriInt = BitConverter.ToInt32(ori, 0);

			int finalChecksum = oriInt ^ finB; // -513519573 final chk die kann ich bekommen

			// finB 1578371036
			// ori int unbekannt

			//int zappa = finalChecksum ^ finB;

			string hexstring = finalChecksum.ToString("X");

			//string hexstring2 = zappa.ToString("X");

			System.Diagnostics.Debug.WriteLine(hexstring); // "9215CB38" 51 10 3B 19







			return null;
		}

		// generate checksum from data, this part is working! maybe the checksum could be wrong reversed
		public byte[] generateChecksumFromData(byte[] data)
		{
			byte[] chksum = new byte[4];

			byte[] tempData = new byte[data.Length];

			Buffer.BlockCopy(data, 0, tempData, 0, data.Length);

			Array.Reverse(tempData);

			int blocks = tempData.Length / 4;
			//int rest = tempData.Length % 4;

			int a = 0;
			int b = 0;
			int c = 0;
			int d = 0;

			int add = tempData.Length;
			b = add;

			for (int i = 0; i < blocks; i++)
			{
				a += tempData[4 * i];
				b += tempData[4 * i + 1];
				c += tempData[4 * i + 2];
				d += tempData[4 * i + 3];
			}


			int v1 = a / 256;
			int v2 = b / 256;
			int v3 = c / 256;
			int v4 = d / 256;

			int v1f = a - (v1 * 256) + v2;
			int v2f = b - (v2 * 256) + v3;
			int v3f = c - (v3 * 256) + v4;
			int v4f = d - (v4 * 256);

			chksum[0] = (byte)v1f;
			chksum[1] = (byte)v2f;
			chksum[2] = (byte)v3f;
			chksum[3] = (byte)v4f;

			return chksum;
		}

		// this isn't correct working because of the seed...
		public byte[] generateChecksumForPacket(byte[] checksum, byte[] seedAdd)
		{
			byte[] checksumPacket = new byte[8];
			byte[] seed = {0x31,0x8e,0xde,0x44};
			byte[] minusSeed = new byte[4];

			Array.Reverse(checksum);

			minusSeed[0] = (byte)(checksum[0] - seed[0]);
			minusSeed[1] = (byte)(checksum[1] - seed[1]);
			minusSeed[2] = (byte)(checksum[2] - seed[2]);
			minusSeed[3] = (byte)(checksum[3] - seed[3]);

			if (checksum[0] - seed[0] < 0)
			{
				minusSeed[1]--;
			}

			if (checksum[1] - seed[1] < 0)
			{
				minusSeed[2]--;
			}

			if (checksum[2] - seed[2] < 0)
			{
				minusSeed[3]--;
			}


			int a = minusSeed[0] + seedAdd[0];
			int b = minusSeed[1] + seedAdd[1];
			int c = minusSeed[2] + seedAdd[2];
			int d = minusSeed[3] + seedAdd[3];

			if (a > 255)
			{
				b++;
			}

			if (b > 255)
			{
				c++;
			}

			if (c > 255)
			{
				d++;
			}


			checksumPacket[0] = (byte)a;
			checksumPacket[1] = (byte)b;
			checksumPacket[2] = (byte)c;
			checksumPacket[3] = (byte)d;

			checksumPacket[4] = seedAdd[0];
			checksumPacket[5] = seedAdd[1];
			checksumPacket[6] = seedAdd[2];
			checksumPacket[7] = seedAdd[3];

			return checksumPacket;
		}
	}
}
