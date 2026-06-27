using System;

namespace Shipeng.Util
{
    public class ShipengException : Exception
    {
        public ShipengException()
        {
        }

        public ShipengException(string message) : base(message)
        {
        }

        public ShipengException(string message,Exception innerException) : base(message, innerException)
        {
        }
    }
}
