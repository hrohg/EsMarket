using System;
using System.Runtime.Serialization;

namespace ES.Business.DataUtilities.Exceptions
{
    public class InvalidConnectionException : Exception
    {
        #region Construction
        public InvalidConnectionException() { }
        public InvalidConnectionException(string message) : base(message) { }
        protected InvalidConnectionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public InvalidConnectionException(string message, Exception innerException) : base(message, innerException) { }
        #endregion
    }
}
