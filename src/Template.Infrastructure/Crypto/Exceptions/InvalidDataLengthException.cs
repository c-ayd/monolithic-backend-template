namespace Template.Infrastructure.Crypto.Exceptions
{
    public class InvalidDataLengthException : Exception
    {
        public InvalidDataLengthException(int dataLength) :
            base($"The data length cannot be negative. Given value: {dataLength}")
        {
        }
    }
}
