/*
 * Short explaination how this class works:
 * 
 * Call 'generateDecryptedPacket(your raw packet in byte[], true if it'S a client packet; false if it's a server packet);'. Returns the decrypted packet.
 * A raw client or server packet is only the "data part" of the udp packet, not the whole packet with checksum, port and ip adress,...
 * The lotro client uses look up tables for decryption as well. They maybe change with newer versions of the client. But, they can easily extracted from hex dump
 * when the client is running. They don't exist in the .exe while not running! Maybe the client loads the values from the zipped files?
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Helper
{

	public class Decrypt
	{

		readonly int[,] jumpTableClient;
		readonly List<byte[][]> lookUpListClient;
		readonly int[,] jumpTableServer;
		readonly List<byte[][]> lookUpListServer;
		int lastIndex; // last index has to be keept til new value is found


		public Decrypt()
		{
			jumpTableClient = HelperMethods.Instance.getJumpTableClient();
			lookUpListClient = HelperMethods.Instance.getLookUpListClient();
			jumpTableServer = HelperMethods.Instance.getJumpTableServer();
			lookUpListServer = HelperMethods.Instance.getLookUpListServer();
		}

		public BEBinaryReader decrypt(BEBinaryReader ber, bool clientPacket)
		{
			MemoryStream memoryStream = new MemoryStream();
			BEBinaryWriter bEBinaryWriter = new BEBinaryWriter(memoryStream, Encoding.UTF8);
			int startPosition = 4;
			try
			{
				while (ber.BaseStream.Position < ber.BaseStream.Length)
				{
					byte[] byte_ = ber.ReadBytes(4);
					byte[] buffer = processBlock(byte_, clientPacket, startPosition);
					startPosition = 0;
					bEBinaryWriter.Write(buffer);
				}
			}
			catch (Exception)
			{
			}
			ber.BaseStream.Close();
			ber.BaseStream.Dispose();
			memoryStream.Position = 0L;
			return new BEBinaryReader(memoryStream, Encoding.UTF8);
		}

		public BEBinaryReader decryptClientPacket(BEBinaryReader ber, ushort sessionID)
		{
			MemoryStream memoryStream = new MemoryStream();
			BEBinaryWriter bEBinaryWriter = new BEBinaryWriter(memoryStream, Encoding.UTF8);
			bEBinaryWriter.WriteUInt16BE(sessionID);
			int startPosition = 4;
			try
			{
				while (ber.BaseStream.Position < ber.BaseStream.Length)
				{
					byte[] byte_ = ber.ReadBytes(4);
					byte[] buffer = this.method_0(byte_, startPosition);
					startPosition = 0;
					bEBinaryWriter.Write(buffer);
				}
			}
			catch (Exception)
			{
			}
			ber.BaseStream.Close();
			ber.BaseStream.Dispose();
			memoryStream.Position = 0L;
			ber = new BEBinaryReader(memoryStream, Encoding.UTF8);
			return ber;
		}

		// decrypts both, server- and client packets
		// true for a client packet
		// false for a server packet
		public byte[] generateDecryptedPacket(byte[] packet, bool isClientPacket)
		{
            // If the first two bytes are 0x00 no encryption has happened yet
            if (packet[0] == 0 && packet[1] == 0)
            {
                // We return a copied packet packet and not the same byte array. This way
                // we make sure the caller isn't surprised when he modifies the returned
                // byte array and his input magically changes as well (byte array are a
                // reference type). Next to that we skip the first two bytes which are
                // basically padding
                byte[] nonDecryptedPacket = new byte[packet.Length - 2];
                Buffer.BlockCopy(packet, 2, nonDecryptedPacket, 0, packet.Length - 2);
                return nonDecryptedPacket;
            }

            // Also if the third byte contains a 0x00, the packet is not encrypted. This
            // may happen from time to time if the server decides to (maybe when under
            // heavy load).
            if (packet[2] == 0)
            {
                // We return a copied packet packet and not the same byte array. This way
                // we make sure the caller isn't surprised when he modifies the returned
                // byte array and his input magically changes as well (byte array are a
                // reference type). We will skip the byte which is padding again.
                byte[] nonDecryptedPacket = new byte[packet.Length - 1];
                Buffer.BlockCopy(packet, 0, nonDecryptedPacket, 0, 2);
                Buffer.BlockCopy(packet, 3, nonDecryptedPacket, 2, packet.Length - 3);
                return nonDecryptedPacket;
            }

			byte[] tempResult = new byte[packet.Length * 16];

			int startPosition = 4; // the first 4 Bits in the first block are skiped
			lastIndex = 0;

			// First 2 Bytes are the same in the decrypted packet, Write them to temp array
			tempResult[0] = packet[0];
			tempResult[1] = packet[1];
			int pos = 2;

			// Decrypt blocks of 4 Byte
			long lengthPacket = packet.Length - 2; // first 2 Bytes already read and have nothing to do with the decrypted packet
			int numberOfBlocks = (int)lengthPacket / 4;
			int lastBlockLength = (int)lengthPacket % 4;

			byte[] block = new byte[4];
			byte[] lastBlock = new byte[lastBlockLength];

			for (int i = 0; i < numberOfBlocks; i++)
			{
				Buffer.BlockCopy(packet, (i*4) + 2, block, 0, 4);
				byte[] decryptedBlock = processBlock(block, isClientPacket, startPosition);
				Buffer.BlockCopy(decryptedBlock, 0, tempResult, pos, decryptedBlock.Length);
				pos += decryptedBlock.Length;
				startPosition = 0;

			}

			// Decrypt last block
			if (lastBlockLength > 0)
			{
				Buffer.BlockCopy(packet, (int)lengthPacket - lastBlockLength + 2, lastBlock, 0, lastBlockLength);
				byte[] decryptedBlock = processBlock(lastBlock, isClientPacket, startPosition);
				Buffer.BlockCopy(decryptedBlock, 0, tempResult, pos, decryptedBlock.Length);
				pos += decryptedBlock.Length;
			}

			// Returns the final decrypted packet
			byte[] decryptedPacket = new byte[pos];
			Buffer.BlockCopy(tempResult, 0, decryptedPacket, 0, pos);
			return decryptedPacket;
		}

		byte[] method_0(byte[] block, int startPosition)
		{
			int num = 16372;
			byte[] tempResult = new byte[block.Length * 16];
			int pos = 0;
			BitArray bitArray = new BitArray(block);
			for (int i = startPosition; i < bitArray.Length; i++)
			{
				int column = bitArray.Get(i) ? 1 : 0;
				if (jumpTableClient[lastIndex, column] >= 0)
				{
					lastIndex = jumpTableClient[lastIndex, column];
				}
				else
				{
					int index = jumpTableClient[lastIndex, column] + num;
					byte[][] lookUpEntry = lookUpListClient[index];
					byte[] lookUpValue = lookUpEntry[0];
					try
					{
						Buffer.BlockCopy(lookUpValue, 0, tempResult, pos, lookUpValue.Length);
					}
					catch (Exception)
					{
					}
					pos += lookUpValue.Length;
					lastIndex = 0;
				}
			}
			byte[] result = new byte[pos];
			Buffer.BlockCopy(tempResult, 0, result, 0, pos);
			return result;
		}

		/*  This is how the decryption is done:
		 * 1. Revers order of the 4 Bytes (Example: 0xEF 0xF5 0x49 0x85 => 0x85 0x49 0xF5 0xEF)
		 * 2. Get bit value of the 4 Byte word (Example:  0x85 0x49 0xF5 0xEF => 10000101010010011111010111101111)
		 * 3. Revers the order again (Example: 10000101010010011111010111101111 => 11110111101011111001001010100001)
		 * 4. Parse the bit array:
		 *
		 *	if there is a 0, use the adress which is given in the first row an first column of the look-up-table
		 *	if there is a 1, use the adress which is given in the first row an second column of the look-up-table
		 *
		 *	Jump to the row of the given adress in the look-up-table
		 *
		 *	Do this for all following bits in the array til you can't jump any further in this table and find a jump to the second look-up-table
		 * 5. Get the value which is given in the second look-up-table
		 * 6. Start with 4 again til there are no bits left in the array
		 */
		byte[] processBlock(byte[] block, bool client, int startPosition)
		{
			int[,] jumpTable;
			List<byte[][]> lookUpList;
			int offset;
			int multiplier;

			if (client)
			{
				jumpTable = jumpTableClient;
				lookUpList = lookUpListClient;
				offset = 16372;
				multiplier = 16;
			}
			else
			{
				jumpTable = jumpTableServer;
				lookUpList = lookUpListServer;
				offset = 16125;
				multiplier = 6;
			}

			byte[] tempResult = new byte[block.Length * multiplier];
			int pos = 0;

			//Array.Reverse(block); BitArray is reversing bytes default

			BitArray bitArray = new BitArray(block);
			for (int i = startPosition; i < bitArray.Length; i++)
			{
				int column = bitArray.Get(i) ? 1 : 0;
				if (jumpTable[lastIndex, column] >= 0)
				{
					lastIndex = jumpTable[lastIndex, column];
				}
				else
				{
					int index = jumpTable[lastIndex, column] + offset;

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
