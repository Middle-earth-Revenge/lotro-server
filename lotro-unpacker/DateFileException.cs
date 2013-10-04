using System;
using System.Runtime.Serialization;
namespace DAT_UNPACKER
{
    internal class DatFileException : ApplicationException
    {
        public DatFileException()
        {
        }
        public DatFileException(string message)
            : base(message)
        {
        }
        public DatFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        public DatFileException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
