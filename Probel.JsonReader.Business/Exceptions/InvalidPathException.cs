using System;
using System.Runtime.Serialization;

namespace Probel.JsonReader.Business.Exceptions
{
    [Serializable]
    public class InvalidPathException : ApplicationException
    {
        #region Constructors

        public InvalidPathException() : this("The specified path is empty or not valid.")
        {
        }

        public InvalidPathException(string message) : base(message)
        {
        }

        public InvalidPathException(string message, System.Exception inner) : base(message, inner)
        {
        }

        protected InvalidPathException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #endregion Constructors
    }
}