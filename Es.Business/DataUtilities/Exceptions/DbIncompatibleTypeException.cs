using System;
using System.Runtime.Serialization;

namespace ES.Business.DataUtilities.Exceptions
{
    public class DbIncompatibleTypeException : Exception
    {
        #region Construction
        public DbIncompatibleTypeException() { }
        public DbIncompatibleTypeException(string message) : base(message) { }
        protected DbIncompatibleTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public DbIncompatibleTypeException(string message, Exception innerException) : base(message, innerException) { }
        #endregion
    }
}
