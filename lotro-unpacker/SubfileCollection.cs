using System;
using System.Collections;
using System.Collections.Generic;
namespace DAT_UNPACKER
{
    public class SubfileCollection : IEnumerable<Subfile>, IEnumerable
    {
        private readonly int _handle = -1;
        internal SubfileCollection(DatFile df)
        {
            this._handle = df.Handle;
        }
        public IEnumerator<Subfile> GetEnumerator()
        {
            return new SubfileEnumerator(this._handle);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SubfileEnumerator(this._handle);
        }
    }
}
