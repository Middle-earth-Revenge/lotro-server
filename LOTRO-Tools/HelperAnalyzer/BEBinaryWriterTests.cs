using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HelperAnalyzer
{
    [TestClass]
    public class BEBinaryWriterTests
    {
        private static string ToString(byte[] packet)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach (byte b in packet)
            {
                sb.AppendFormat("{0:x2}", b);
                count++;
                if (count % 2 == 0)
                {
                    sb.Append(' ');
                }
                if (count % 16 == 0 && count != packet.Length)
                {
                    sb.Append("\r\n");
                }
            }
            return sb.ToString();
        }

        [TestMethod]
        public void testWriteUInt16BE()
        {
            MemoryStream stream = new MemoryStream();
            Helper.BEBinaryWriter writer = new Helper.BEBinaryWriter(stream, Encoding.UTF8);

            writer.WriteUInt16BE(0x0102);
            Assert.AreEqual(ToString(new byte[] { 0x01, 0x02 }), ToString(stream.ToArray()));
            writer.WriteUInt16BE(0x0304);
            Assert.AreEqual(ToString(new byte[] { 0x01, 0x02, 0x03, 0x04 }), ToString(stream.ToArray()));
        }

        [TestMethod]
        public void testWriteUInt32BE()
        {
            MemoryStream stream = new MemoryStream();
            Helper.BEBinaryWriter writer = new Helper.BEBinaryWriter(stream, Encoding.UTF8);

            writer.WriteUInt32BE(0x01020304);
            Assert.AreEqual(ToString(new byte[] { 0x01, 0x02, 0x03, 0x04 }), ToString(stream.ToArray()));
            writer.WriteUInt32BE(0x05060708);
            Assert.AreEqual(ToString(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }), ToString(stream.ToArray()));
        }

        [TestMethod]
        public void testWriteUInt64BE()
        {
            MemoryStream stream = new MemoryStream();
            Helper.BEBinaryWriter writer = new Helper.BEBinaryWriter(stream, Encoding.UTF8);

            writer.WriteUInt64BE(0x0102030405060708);
            Assert.AreEqual(ToString(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }), ToString(stream.ToArray()));
            writer.WriteUInt64BE(0x090A0B0C0D0E0F10);
            Assert.AreEqual(ToString(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 }), ToString(stream.ToArray()));
        }

        // FIXME: it seems like this is not implemented?!?
        // [TestMethod]
        public void testWriteUnicodeString()
        {
            MemoryStream stream = new MemoryStream();
            Helper.BEBinaryWriter writer = new Helper.BEBinaryWriter(stream, Encoding.UTF8);

            writer.WriteUnicodeString("A");
            Assert.AreEqual(ToString(new byte[] { 0x01, 0x00, 0x41 }), ToString(stream.ToArray()));
            writer.WriteUnicodeString("BC");
            Assert.AreEqual(ToString(new byte[] { 0x01, 0x00, 0x41, 0x02, 0x00, 0x42, 0x00, 0x43 }), ToString(stream.ToArray()));
        }
    }
}
