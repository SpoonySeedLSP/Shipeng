using System;

namespace Shipeng.Util
{
    public class ShipengConvertException : ShipengException
    {
        public ShipengConvertException()
        {
        }

        public ShipengConvertException(string message) : base(message)
        {
        }

        public ShipengConvertException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}