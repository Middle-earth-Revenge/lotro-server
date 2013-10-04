using System;
namespace DAT_UNPACKER
{
    [Flags]
    public enum Options
    {
        None = 0,
        LoadWav = 1,
        LoadOgg = 2,
        LoadJpg = 4,
        LoadDds = 8,
        LoadHks = 16,
        LoadBin = 256,
        ExtractSelectedFile = 512,
        GenerateFileList = 1024,
        ExtractRawFile = 2048,
        LoadAllFiles = 287
    }
}
