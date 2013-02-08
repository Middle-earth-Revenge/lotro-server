/*
 * Short explaination how this class works:
 * 
 * Call 'generateEncryptedPacket(your raw packet in byte[], true if it'S a client packet; false if it's a server packet);'. Returns the encrypted packet.
 * The lotro client uses look up tables for encryption as well. They maybe change with newer versions of the client. But, they can easily extracted from hex dump
 * when the client is running. They don't exist in the .exe while not running! Maybe the client loads the values from the zipped files?
 * 
 * The difficult part with encryption was to generate the correct first byte after the "server signature", e.g. 0xf2 0x00.
 * 
*/


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace LOTRO
{
	class Encrypt
	{
		private List<byte[][]> lookUpList;
		private byte[][] quickLookUpArray;
		//private int startPosition;
		//private int lastIndex = 0; // last index has to be keept til new value is found

		public Encrypt()
		{
			this.lookUpList = HelperMethods.Instance.getLookUpListServer();
			this.quickLookUpArray = HelperMethods.Instance.getQuickLookUpListArrayServer();
		}

		public byte[] generateEncryptedPacket(byte[] data, bool isClient)
		{

			byte[] tempResult = new byte[data.Length - 2];
			// Returns the final encrypted packet
			byte[] encryptedPacket = null;
			MemoryStream msEncrypted = new MemoryStream(); // only simple coded
			MemoryStream allBits = new MemoryStream(); // must be replaced in a final version with a fast structure

			// must be something and doesn't matter first
			// this can only be fixed later in source code if you got the whole encrypted part parsed. the client does this too.
			allBits.WriteByte(0);
			allBits.WriteByte(0);
			allBits.WriteByte(0);
			allBits.WriteByte(0);

			byte firstByte = data[0];
			byte secondByte = data[1];

			msEncrypted.WriteByte(firstByte);
			msEncrypted.WriteByte(secondByte);

			Buffer.BlockCopy(data, 2, tempResult, 0, data.Length - 2);

			bool clear = false;
			//bool lastBlock = false;

			int index = 0;
			byte[] lookUp = null;

			int lastIndex = 0;

			//byte[] xxx = new byte[4];

			int countBits = 0;

			int theEndLeft = 0;

			int theEndRight = 4;

			for (int i = 0; i < tempResult.Length; i++)
			{

				int arrayLength = 1;

				/* a little strange coding, maybe someone could make it easier
                 * 
                 * Take one byte, get its index at quickLookUpArray
                 * look inside the lookUpTable if at that index a end value appears(in original there was a jump to a memory adress)
                 * if no, take the index
                 * if yes and there are many bytes left, take the first byte together with the next byte and so one(up to 4 bytes in a row), til no endvalue found
                */
				while (!clear)
				{
					lookUp = new byte[arrayLength];

					for (int pos = 0; pos < arrayLength; pos++)
						lookUp[pos] = tempResult[i + pos];

					index = HelperMethods.Instance.getIndexFromByte(quickLookUpArray, lookUp);

					if (index == -1)
					{
						clear = true;
						index = lastIndex;
					}
					else
					{

						lastIndex = index;

						clear = HelperMethods.Instance.checkForEndValue(index, isClient);

						// check for last bytes
						if (i + lookUp.Length == tempResult.Length)
						{
							clear = true;
							//lastBlock = true;
						}

						arrayLength++;
					}
				}

				i += arrayLength - 2;

				clear = false;

				if (index != -1)
				{

					// process value found at index in lookUpTable
					byte[][] entry = lookUpList[index];

					int encodedLength = entry[1][0]; // the length for encoding

					theEndRight += encodedLength;

					if (theEndRight >= 32)
					{
						theEndLeft++;

						theEndRight = theEndRight - 32;
					}

					countBits += encodedLength;

					BitArray encodedValue = new BitArray(entry[2]);

					for (int j = 0; j < encodedLength; j++)
						if (encodedValue[j] == true)
							allBits.WriteByte(1);
					else
						allBits.WriteByte(0);

				}

			}


			byte[] final = allBits.ToArray();

			byte[] block = new byte[8];

			BitArray ba = new BitArray(8);

			int blocks = (final.Length / 8);

			int rest = final.Length % 8;

			for (int count = 0; count < blocks; count++)
			{

				Buffer.BlockCopy(final, count * 8, block, 0, 8);


				for (int i = 0; i < block.Length; i++)
					if (block[i] == 1)
						ba.Set(i, true);
				else
					ba.Set(i, false);

				int[] array = new int[1];
				ba.CopyTo(array, 0);
				int value = array[0];

				msEncrypted.WriteByte((byte)value);
			}

			if (rest > 0)
			{
				block = new byte[rest];

				ba = new BitArray(rest);

				Buffer.BlockCopy(final, blocks * 8, block, 0, rest);


				for (int i = 0; i < block.Length; i++)
					if (block[i] == 1)
						ba.Set(i, true);
				else
					ba.Set(i, false);

				int[] lastArray = new int[1];
				ba.CopyTo(lastArray, 0);
				int lastValue = lastArray[0];

				msEncrypted.WriteByte((byte)lastValue);
			}

			encryptedPacket = msEncrypted.ToArray();

			// at this point the beginning of the encrypted packet is fixed
			// this was the most difficult part for me and took me a lot of time to investigate
			// if this part is missing you never get a correct encrypted packet
			int maxBits = (encryptedPacket.Length - 2) * 8;
			int realBits = (theEndLeft << 5) + theEndRight;

			int valueToAdd = ((maxBits - realBits) * 2) + 1;

			encryptedPacket[2] += (byte)valueToAdd;





			return encryptedPacket;

		}






	}
}
