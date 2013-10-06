using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HelperAnalyzer
{
    [TestClass]
    public class HelperMethodsTests
    {
        [TestMethod]
        public void testInstance()
        {
            Assert.IsNotNull(Helper.HelperMethods.Instance);
        }

        [TestMethod]
        public void testArrayEqual()
        {
            byte[] EMPTY = new byte[] { };
            byte[] SINGLE = new byte[] { 0x01 };

            // Comapre against the same array
            Assert.AreEqual(true, Helper.HelperMethods.Instance.ArraysEqual(EMPTY, EMPTY));
            Assert.AreEqual(true, Helper.HelperMethods.Instance.ArraysEqual(SINGLE, SINGLE));
            Assert.AreEqual(false, Helper.HelperMethods.Instance.ArraysEqual(EMPTY, SINGLE));
            Assert.AreEqual(false, Helper.HelperMethods.Instance.ArraysEqual(SINGLE, EMPTY));

            // Comapre against an identical array
            Assert.AreEqual(true, Helper.HelperMethods.Instance.ArraysEqual(EMPTY, new byte[] { }));
            Assert.AreEqual(true, Helper.HelperMethods.Instance.ArraysEqual(new byte[] { }, EMPTY));
            Assert.AreEqual(true, Helper.HelperMethods.Instance.ArraysEqual(SINGLE, new byte[] { 0x01 }));
            Assert.AreEqual(true, Helper.HelperMethods.Instance.ArraysEqual(new byte[] { 0x01 }, SINGLE));
        }

        [TestMethod]
        public void testCheckForEndValueClient()
        {
            // TODO: add a small limit for now. This likely reads one of the files in Data
            for (int i = 0; i <= 0x1110 /* int.MaxValue */; i++)
            {
                bool expected = true;
                if (i == 0x0000 ||
                    i == 0x0001 ||
                    i == 0x0002 ||
                    i == 0x010A ||
                    i == 0x0282 ||
                    i == 0x0397 ||
                    i == 0x042F ||
                    i == 0x0430 ||
                    i == 0x0474 ||
                    i == 0x066C ||
                    i == 0x066D ||
                    i == 0x066E ||
                    i == 0x0916 ||
                    i == 0x0917 ||
                    i == 0x0918 ||
                    i == 0x0B70 ||
                    i == 0x0B71 ||
                    i == 0x0BF9 ||
                    i == 0x0BFE ||
                    i == 0x0DCA ||
                    i == 0x0DCB ||
                    i == 0x0DCC ||
                    i == 0x1010 ||
                    i == 0x1110)
                {
                    expected = false;
                }
                Assert.AreEqual(expected, Helper.HelperMethods.Instance.checkForEndValue(i, true), "i=" + i + " (0x" + i.ToString("X") + ")");
            }
        }

        [TestMethod]
        public void testCheckForEndValueServer()
        {
            // TODO: add a small limit for now. This likely reads one of the files in Data
            for (int i = 0; i <= 0x1110 /* int.MaxValue */; i++)
            {
                bool expected = true;
                if (i == 0x0000 ||
                    i == 0x0001 ||
                    i == 0x0002 ||
                    i == 0x0103 ||
                    i == 0x01FB ||
                    i == 0x03E6 ||
                    i == 0x03E7 ||
                    i == 0x05C7 ||
                    i == 0x06C9 ||
                    i == 0x06CA ||
                    i == 0x0896 ||
                    i == 0x09AC ||
                    i == 0x0B64 ||
                    i == 0x0B65 ||
                    i == 0x0B66 ||
                    i == 0x0E65 ||
                    i == 0x0E66 ||
                    i == 0x0F68)
                {
                    expected = false;
                }
                Assert.AreEqual(expected, Helper.HelperMethods.Instance.checkForEndValue(i, false), "i=" + i + " (0x" + i.ToString("X") + ")");
            }
        }

        // FIXME [TestMethod]
        public void testExtractSessionKeyFrom1stClientPacket()
        {
            byte[] data = new byte[0];
            Helper.HelperMethods.Instance.extractSessionKeyFrom1stClientPacket(data);
        }

        [TestMethod]
        public void testGenerateChecksumForHeader()
        {
            ushort sessionID = 0x00f1;
            ushort dataLength = 8;
            byte[] action = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
            uint sequenceNumber = 1;
            uint dataChecksum = 0;
            uint ackNr = 2;
            Assert.AreEqual(0xC0E877F0, Helper.HelperMethods.Instance.generateChecksumForHeader(sessionID, dataLength, action, sequenceNumber, dataChecksum, ackNr));

            dataChecksum = 1;
            Assert.AreEqual(0xBFE572EA, Helper.HelperMethods.Instance.generateChecksumForHeader(sessionID, dataLength, action, sequenceNumber, dataChecksum, ackNr));
            dataChecksum = 0;

            sequenceNumber = 2;
            Assert.AreEqual(0xC0E877F1, Helper.HelperMethods.Instance.generateChecksumForHeader(sessionID, dataLength, action, sequenceNumber, dataChecksum, ackNr));
            sequenceNumber = 1;

            ackNr = 1;
            Assert.AreEqual(0xBFE572E8, Helper.HelperMethods.Instance.generateChecksumForHeader(sessionID, dataLength, action, sequenceNumber, dataChecksum, ackNr));
            ackNr = 2;
        }

        [TestMethod]
        public void testgetChecksumFromData()
        {
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void testgetChecksumFromHeader()
        {
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void testgetDefaultHeadChecksum()
        {
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void testgetIndexFromByte()
        {
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void testgetJumpTableClient()
        {
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void testgetJumpTableServer()
        {
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void testgetLookUpListClient()
        {
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void testgetLookUpListServer()
        {
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void testgetQuickLookUpListArrayServer()
        {
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void testRC4()
        {
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void testRC4ToBytes()
        {
            Assert.Fail("Not implemented");
        }
        [TestMethod]
        public void testwriteLog()
        {
            Assert.Fail("Not implemented");
        }
    }

}
