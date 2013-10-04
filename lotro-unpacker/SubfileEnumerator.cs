using System;
using System.Collections;
using System.Collections.Generic;
namespace DAT_UNPACKER
{
    public class SubfileEnumerator : IEnumerator<Subfile>, IDisposable, IEnumerator
    {
        private int _currentIndex = -1;
        private readonly int _handle = -1;
        private readonly int _numSubFiles;
        public Subfile Current
        {
            get
            {
                return this.GetCurrentSubfile();
            }
        }
        object IEnumerator.Current
        {
            get
            {
                return this.GetCurrentSubfile();
            }
        }
        internal SubfileEnumerator(int handle)
        {
            this._handle = handle;
            this._numSubFiles = DatExport.GetNumSubfiles(this._handle);
        }
        public void Dispose()
        {
        }
        private Subfile GetCurrentSubfile()
        {
            int did;
            int size;
            int iteration;
            DatExport.GetSubfileSizes(this._handle, out did, out size, out iteration, this._currentIndex, 1);
            return new Subfile(this._handle, did, size, iteration, DatExport.GetSubfileVersion(this._handle, did));
        }
        public bool MoveNext()
        {
            if (this._currentIndex < this._numSubFiles - 1)
            {
                this._currentIndex++;
                return true;
            }
            return false;
        }
        public void Reset()
        {
            this._currentIndex = 0;
        }
    }
}
