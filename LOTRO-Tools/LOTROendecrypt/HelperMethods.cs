using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections;

namespace LOTROendecrypt
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
		//private readonly string fileNameTableJumpRaw = "data\\table_jump_raw";
		private readonly string fileNameTableJump = "data\\table_jump";
		private readonly string fileNameTableLookUpRaw = "data\\table_lookup_raw";
		private readonly string fileNameTableLookUp = "data\\table_lookup";

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

			// for server packets
			//this.jumpTable = generateJumpTableFileFromRaw(fileNameTableJumpRaw, fileNameTableJump); // only when jump table is not there and needs to gain from lotro.exe hex dump
			this.jumpTableServer = generateJumpTable(fileNameTableJump);
			//quickLookUp = new byte[16125][]; // there are 16125 values
			quickLookUpServer = new byte[16125][]; // there are 16125 values
			this.lookUpListServer = generateLookUpTableFileFromRaw(fileNameTableLookUpRaw, fileNameTableLookUp); // only when look-up table is not there and needs to gain from lotro.exe hex dump
			//this.lookUpList = generateLookUpTableFile(fileNameTableLookUp);

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

		public int[,] getJumpTableServer()
		{
			return this.jumpTableServer;
		}

		public List<byte[][]> getLookUpListServer()
		{
			return this.lookUpListServer;
		}

		public byte[][] getQuickLookUpListArray()
		{
			return this.quickLookUpServer;
		}

		private int[,] generateJumpTable(string fileInputName)
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

		private int[,] generateJumpTableFileFromRaw(string fileInputName, string fileOutputName)
		{

			FileStream fsRead = new FileStream(@fileInputName, FileMode.Open);

			int[,] jumpTable = new Int32[fsRead.Length / 16, 2];

			byte[] adress0 = new byte[4];
			byte[] adress1 = new byte[4];

			FileStream fsWrite = new FileStream(@fileOutputName, FileMode.Create);

			bool firstValueRead = false;
			int startAdressMemory = 0;

			for (int i = 0; i < jumpTable.Length / 2; i++)
			{

				fsRead.ReadByte();
				fsRead.ReadByte();
				fsRead.ReadByte();
				fsRead.ReadByte();

				fsRead.Read(adress0, 0, 4);
				fsRead.Read(adress1, 0, 4);

				fsRead.ReadByte();
				fsRead.ReadByte();
				fsRead.ReadByte();
				fsRead.ReadByte();

				if (!firstValueRead)
				{
					startAdressMemory = (BitConverter.ToInt32(adress0, 0) - 16);

					firstValueRead = true;

				}

				int value0 = (BitConverter.ToInt32(adress0, 0) - startAdressMemory) / 16;
				int value1 = (BitConverter.ToInt32(adress1, 0) - startAdressMemory) / 16;

				jumpTable[i, 0] = value0;
				jumpTable[i, 1] = value1;

				// for saving it to a file
				byte[] first = BitConverter.GetBytes(value0);
				byte[] second = BitConverter.GetBytes(value1);


				fsWrite.Write(first, 0, first.Length);
				fsWrite.Write(second, 0, second.Length);


			}


			fsWrite.Close();
			fsRead.Close();

			return jumpTable;
		}

		private List<byte[][]> generateLookUpTableFile(string fileInputName)
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

		private List<byte[][]> generateLookUpTableFileFromRaw(string fileInputName, string fileOutputName)
		{
			FileStream fsRead = new FileStream(@fileInputName, FileMode.Open);

			List<byte[][]> tempLookUpList = new List<byte[][]>();

			byte[][] entry;

			byte[] cipher;// = new byte[4];
			byte[] encodingLength;
			byte[] encryptArray;
			byte[] endValue;

			FileStream fsWrite = new FileStream(@fileOutputName, FileMode.Create);

			for (int i = 0; i < fsRead.Length / 16; i++)
			{
				encodingLength = new byte[1];
				encryptArray = new byte[4];
				endValue = new byte[4];

				fsRead.ReadByte(); // skip
				int length = fsRead.ReadByte();
				fsRead.Read(encodingLength,0,1);
				fsRead.ReadByte(); // skip

				cipher = new byte[length];

				fsRead.Read(cipher, 0, length);

				for (int j = length; j < 4; j++)
					fsRead.ReadByte(); // skip

				fsRead.Read(endValue, 0, 4);
				fsRead.Read(encryptArray, 0, 4);

				entry = new byte[4][];

				entry[0] = cipher;
				entry[1] = encodingLength;
				entry[2] = encryptArray;
				entry[3] = endValue;

				tempLookUpList.Add(entry);
				quickLookUpServer[i] = cipher;

				fsWrite.WriteByte((byte)length);
				fsWrite.Write(cipher, 0, cipher.Length);
				fsWrite.Write(encodingLength,0,1);
				fsWrite.Write(encryptArray, 0, encryptArray.Length);
				fsWrite.Write(endValue, 0, endValue.Length);

			}

			fsWrite.Close();
			fsRead.Close();

			return tempLookUpList;
		}

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

	}
}
