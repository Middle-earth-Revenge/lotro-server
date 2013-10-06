using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HelperAnalyzer
{
    [TestClass]
    public class BEBinaryReaderTests
    {
        [TestMethod]
        public void testReadUInt16BE()
        {
            byte[] data = new byte[4];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte) (i+1);
            }
            MemoryStream stream = new MemoryStream(data);
            Helper.BEBinaryReader reader = new Helper.BEBinaryReader(stream, Encoding.UTF8);

            ushort tmp = reader.ReadUInt16BE();
            Assert.AreEqual(258, tmp);
            tmp = reader.ReadUInt16BE();
            Assert.AreEqual(772, tmp);
        }
    }
}
