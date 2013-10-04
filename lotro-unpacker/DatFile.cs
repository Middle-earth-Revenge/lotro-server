using System;
using System.Collections.Generic;
using System.Text;
namespace DAT_UNPACKER
{
    public class DatFile : IDisposable
    {
        private long _cachedSize;
        private bool _disposed;
        private int _handle;
        private static readonly bool[] HandleAllocArray;
        private static readonly object HandleAllocArrayLock;
        private readonly bool _isReadOnly;
        private SubfileCollection _subfileCollection;
        public Dictionary<int, Subfile> Files;
        public Subfile this[int index]
        {
            get
            {
                return this.Files[index];
            }
        }
        public long CachedSize
        {
            get
            {
                if (this._cachedSize == -1L)
                {
                    this._cachedSize = 0L;
                    if (this._isReadOnly)
                    {
                        foreach (Subfile current in this._subfileCollection)
                        {
                            this._cachedSize += (long)current.Size;
                        }
                    }
                }
                return this._cachedSize;
            }
        }
        internal int Handle
        {
            get
            {
                return this._handle;
            }
        }
        public SubfileCollection Subfiles
        {
            get
            {
                if (!this._isReadOnly)
                {
                    throw new DatFileException("Unable to iterate the subfiles of a writable dat file");
                }
                return this._subfileCollection;
            }
        }
        static DatFile()
        {
            DatFile.HandleAllocArray = new bool[64];
            DatFile.HandleAllocArrayLock = new object();
            for (int i = 0; i < DatFile.HandleAllocArray.Length; i++)
            {
                DatFile.HandleAllocArray[i] = false;
            }
        }
        private DatFile()
        {
            this._subfileCollection = null;
            this._isReadOnly = true;
            this._handle = -1;
            this._cachedSize = -1L;
            this._disposed = false;
            this.Files = new Dictionary<int, Subfile>();
        }
        public void Load()
        {
            foreach (Subfile current in this._subfileCollection)
            {
                this.Files.Add(current.DataID, current);
            }
        }
        private static int AllocHandle()
        {
            int result;
            lock (DatFile.HandleAllocArrayLock)
            {
                for (int i = 0; i < DatFile.HandleAllocArray.Length; i++)
                {
                    if (!DatFile.HandleAllocArray[i])
                    {
                        DatFile.HandleAllocArray[i] = true;
                        result = i;
                        return result;
                    }
                }
                throw new DatFileException("Too many dat files are already open");
            }
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    DatExport.CloseDatFile(this._handle);
                    DatFile.FreeHandle(this._handle);
                }
                this._disposed = true;
            }
        }
        ~DatFile()
        {
            this.Dispose(false);
        }
        private static void FreeHandle(int handle)
        {
            DatFile.HandleAllocArray[handle] = false;
        }
        public static DatFile OpenExisting(DatFileInitParams initParams)
        {
            int handle = DatFile.AllocHandle();
            DatFile result;
            try
            {
                DatFile datFile = new DatFile
                {
                    _handle = handle
                };
                StringBuilder datIdStamp = new StringBuilder(64);
                StringBuilder firstIterGuid = new StringBuilder(64);
                uint num = 130u;
                if (initParams.IsReadOnly)
                {
                    num |= 4u;
                }
                int num2;
                int num3;
                int num4;
                int num5;
                ulong num6;
                if (DatExport.OpenDatFile(handle, initParams.FileName, num, out num2, out num3, out num4, out num5, out num6, datIdStamp, firstIterGuid) == -1)
                {
                    throw new DatFileException("Unable to open file [ " + initParams.FileName + " ]");
                }
                datFile._subfileCollection = new SubfileCollection(datFile);
                result = datFile;
                datFile.Load();
            }
            catch
            {
                DatFile.FreeHandle(handle);
                throw;
            }
            return result;
        }
    }
}
