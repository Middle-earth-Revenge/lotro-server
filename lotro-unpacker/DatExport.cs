using System;
using System.Runtime.InteropServices;
using System.Text;
namespace DAT_UNPACKER
{
    internal class DatExport
    {
        public const uint DcofCreate = 8u;
        public const uint DcofCreateIfNeeded = 16u;
        public const uint DcofExpandable = 2u;
        public const uint DcofFreeThreaded = 64u;
        public const uint DcofJournalled = 256u;
        public const uint DcofLoadIterations = 128u;
        public const uint DcofOptionalFile = 32u;
        public const uint DcofReadOnly = 4u;
        public const uint DcofSkipIndexCheck = 512u;
        public const uint DcofUseLRU = 1u;
        [DllImport("DatExport.dll")]
        public static extern void CloseDatFile(int handle);
        [DllImport("DatExport.dll")]
        public static extern int GetNumSubfiles(int handle);
        [DllImport("DatExport.dll")]
        public static extern byte GetSubfileCompressionFlag(int handle, int id);
        [DllImport("DatExport.dll")]
        public static extern int GetSubfileData(int handle, int did, IntPtr buffer, int writeOffset, out int version);
        [DllImport("DatExport.dll")]
        public static extern void GetSubfileSizes(int handle, out int did, out int size, out int iteration, int offset, int count);
        [DllImport("DatExport.dll")]
        public static extern int GetSubfileVersion(int handle, int did);
        [DllImport("DatExport.dll", EntryPoint = "OpenDatFileEx2")]
        public static extern int OpenDatFile(int handle, string fileName, uint flags, out int didMasterMap, out int blockSize, out int vnumDatFile, out int vnumGameData, out ulong datFileID, [MarshalAs(UnmanagedType.LPStr)] StringBuilder datIdStamp, [MarshalAs(UnmanagedType.LPStr)] StringBuilder firstIterGuid);
        [DllImport("zlib1T")]
        public static extern int uncompress(byte[] dest, ref int destLen, byte[] source, int sourceLen);
    }
}
