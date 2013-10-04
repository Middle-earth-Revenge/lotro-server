using System;
namespace DAT_UNPACKER
{
    public class DatFileInitParams : ICloneable
    {
        private string _fullFileName = string.Empty;
        private bool _readOnly = true;
        public string FileName
        {
            get
            {
                return this._fullFileName;
            }
            set
            {
                this._fullFileName = value;
            }
        }
        public bool IsReadOnly
        {
            get
            {
                return this._readOnly;
            }
            set
            {
                this._readOnly = value;
            }
        }
        public DatFileInitParams(string filename, bool readOnly)
        {
            this._fullFileName = filename;
            this._readOnly = readOnly;
        }
        public DatFileInitParams()
        {
        }
        public object Clone()
        {
            return new DatFileInitParams
            {
                _readOnly = this._readOnly,
                _fullFileName = this._fullFileName.Clone() as string
            };
        }
    }
}
