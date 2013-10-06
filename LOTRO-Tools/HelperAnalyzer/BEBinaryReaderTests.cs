using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HelperAnalyzer
{
    [TestClass]
    public class BEBinaryReaderTests
    {
        private static Helper.BEBinaryReader InitReader()
        {
            byte[] data = new byte[16];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(i + 1);
            }
            MemoryStream stream = new MemoryStream(data);
            return new Helper.BEBinaryReader(stream, Encoding.UTF8);
        }

        [TestMethod]
        public void testReadUInt16BE()
        {
            Helper.BEBinaryReader reader = InitReader();

            ushort tmp = reader.ReadUInt16BE();
            Assert.AreEqual((ushort) 0x0102, tmp);
            tmp = reader.ReadUInt16BE();
            Assert.AreEqual((ushort)0x0304, tmp);
        }

        [TestMethod]
        public void testReadUInt32BE()
        {
            Helper.BEBinaryReader reader = InitReader();

            uint tmp = reader.ReadUInt32BE();
            Assert.AreEqual((uint) 0x01020304, tmp);
            tmp = reader.ReadUInt32BE();
            Assert.AreEqual((uint)0x05060708, tmp);
        }

        [TestMethod]
        public void testReadUInt64BE()
        {
            Helper.BEBinaryReader reader = InitReader();

            ulong tmp = reader.ReadUInt64BE();
            Assert.AreEqual((ulong)0x0102030405060708, tmp);
            tmp = reader.ReadUInt64BE();
            Assert.AreEqual((ulong)0x090A0B0C0D0E0F10, tmp);
        }

        [TestMethod]
        public void testReadUnicodeString()
        {
            byte[] data = new byte[16];
            data[0] = 1;
            data[1] = 0x41;
            data[2] = 0x00;
            data[3] = 2;
            data[4] = 0x42;
            data[5] = 0x00;
            data[6] = 0x43;
            data[7] = 0x00;
            MemoryStream stream = new MemoryStream(data);
            Helper.BEBinaryReader reader = new Helper.BEBinaryReader(stream, Encoding.UTF8);

            string tmp = reader.ReadUnicodeString();
            Assert.AreEqual("A", tmp);
            tmp = reader.ReadUnicodeString();
            Assert.AreEqual("BC", tmp);
        }
    }
}
