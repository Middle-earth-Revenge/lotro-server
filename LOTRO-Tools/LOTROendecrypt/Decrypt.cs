/*
 * Short explaination how this class works:
 * 
 * Call 'generateDecryptedPacket(your raw packet in byte[], true if it'S a client packet; false if it's a server packet);'. Returns the decrypted packet.
 * A raw client or server packet is only the "data part" of the udp packet, not the whole packet with checksum, port and ip adress,...
 * The lotro client uses look up tables for decryption as well. They maybe change with newer versions of the client. But, they can easily extracted from hex dump
 * when the client is running. They don't exist in the .exe while not running! Maybe the client loads the values from the zipped files?
 * 
 */


﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace LOTROendecrypt
{

	class Decrypt
	{

		private int[,] jumpTableClient;
		private List<byte[][]> lookUpListClient;
		private int[,] jumpTableServer;
		private List<byte[][]> lookUpListServer;
		private int startPosition;
		private int lastIndex; // last index has to be keept til new value is found


		public Decrypt()
		{
			this.jumpTableClient = HelperMethods.Instance.getJumpTableClient();
			this.lookUpListClient = HelperMethods.Instance.getLookUpListClient();
			this.jumpTableServer = HelperMethods.Instance.getJumpTableServer();
			this.lookUpListServer = HelperMethods.Instance.getLookUpListServer();
		}

		// decrypts both, server- and client packets
		// true for a client packet
		// false for a server packet
		public byte[] generateDecryptedPacket(byte[] packet, bool isClientPacket)
		{
			byte[] tempResult = new byte[packet.Length * 4];
			int pos = 0;
			startPosition = 4; // the first 4 Bits in the first block are skiped
			lastIndex = 0;

			// Returns the final decrypted packet
			byte[] decryptedPacket = null;

			// First 2 Bytes are the same in the decrypted packet
			byte firstByte = packet[0];
			byte secondByte = packet[1];

			// Write them to temp array
			tempResult[0] = firstByte;
			tempResult[1] = secondByte;
			pos = 2;

			// Decrypt blocks of 4 Byte
			long lengthPacket = packet.Length - 2; // first 2 Bytes already read and have nothing to do with the decrypted packet
			int numberOfBlocks = (int)lengthPacket / 4;
			int lastBlockLength = (int)lengthPacket % 4;

			byte[] block = new byte[4];
			byte[] lastBlock = new byte[lastBlockLength];

			for (int i = 0; i < numberOfBlocks; i++)
			{
				Buffer.BlockCopy(packet, (i*4) + 2, block, 0, 4);
				byte[] decryptedBlock = processBlock(block, isClientPacket);
				Buffer.BlockCopy(decryptedBlock, 0, tempResult, pos, decryptedBlock.Length);
				pos += decryptedBlock.Length;
				startPosition = 0;

			}

			// Decrypt last block
			if (lastBlockLength > 0)
			{
				Buffer.BlockCopy(packet, (int)lengthPacket - lastBlockLength +2, lastBlock, 0, lastBlockLength);
				byte[] decryptedBlock = processBlock(lastBlock, isClientPacket);
				Buffer.BlockCopy(decryptedBlock, 0, tempResult, pos, decryptedBlock.Length);
				pos += decryptedBlock.Length;
			}

			decryptedPacket = new byte[pos];
			Buffer.BlockCopy(tempResult, 0, decryptedPacket, 0, pos);

			return decryptedPacket;

		}

		/*  This is how the decryption is done:
         * 1. Revers order of the 4 Bytes (Example: 0xEF 0xF5 0x49 0x85 => 0x85 0x49 0xF5 0xEF)
         * 2. Get bit value of the 4 Byte word (Example:  0x85 0x49 0xF5 0xEF => 10000101010010011111010111101111)
         * 3. Revers the order again (Example: 10000101010010011111010111101111 => 11110111101011111001001010100001)
         * 4. Parse the bit array:
         *    
         *    if there is a 0, use the adress which is given in the first row an first column of the look-up-table
         *    if there is a 1, use the adress which is given in the first row an second column of the look-up-table
         *    
         *    Jump to the row of the given adress in the look-up-table
         *    
         *    Do this for all following bits in the array til you can't jump any further in this table and find a jump to the second look-up-table
         * 5. Get the value which is given in the second look-up-table
         * 6. Start with 4 again til there are no bits left in the array
         */
		private byte[] processBlock(byte[] block, bool client)
		{
			int[,] jumpTable;
			List<byte[][]> lookUpList;
			int offset;
			int multiplier;

			if (client)
			{
				jumpTable = this.jumpTableClient;
				lookUpList = this.lookUpListClient;
				offset = 16372;
				multiplier = 16;
			}
			else
			{
				jumpTable = this.jumpTableServer;
				lookUpList = this.lookUpListServer;
				offset = 16125;
				multiplier = 6;
			}

			byte[] tempResult = new byte[block.Length * multiplier];
			int pos = 0;

			//Array.Reverse(block); BitArray is reversing bytes default

			BitArray bitArray = new BitArray(block);

			int index = 0;

			for (int i = startPosition; i < bitArray.Length; i++)
			{
				int column = bitArray.Get(i) ? 1 : 0;

				if (jumpTable[lastIndex, column] >= 0)
				{
					lastIndex = jumpTable[lastIndex, column];
				}
				else
				{
					index = jumpTable[lastIndex, column] + offset;

					byte[][] lookUpEntry = lookUpList[index];
					byte[] lookUpValue = lookUpEntry[0];

					try
					{
						Buffer.BlockCopy(lookUpValue, 0, tempResult, pos, lookUpValue.Length);
					}
					catch (Exception e)
					{

						System.Diagnostics.Debug.WriteLine(e);
					}

					pos += lookUpValue.Length;

					lastIndex = 0;
				}                  

			}

			byte[] result = new byte[pos];

			Buffer.BlockCopy(tempResult, 0, result, 0, pos);

			return result;
		}


	}
}
