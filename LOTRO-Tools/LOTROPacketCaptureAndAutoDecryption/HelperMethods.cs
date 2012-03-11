using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace LOTROPacketCaptureAndAutoDecryption
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

            int v1 = jumpTable[16123, 0];
            int v2 = jumpTable[16123, 0];

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

            int v1 = jumpTable[16123, 0];
            int v2 = jumpTable[16123, 0];

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

    }
}
