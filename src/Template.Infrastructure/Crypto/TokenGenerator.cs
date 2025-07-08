using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using Template.Application.Abstractions.Crypto;
using Template.Infrastructure.Crypto.Exceptions;

namespace Template.Infrastructure.Crypto
{
    public class TokenGenerator : ITokenGenerator
    {
        public string Generate(int dataLength = 32)
        {
            if (dataLength < 0)
                throw new InvalidDataLengthException(dataLength);

            if (dataLength == 0)
                return string.Empty;

            byte[] data = new byte[dataLength];
            using var rnd = RandomNumberGenerator.Create();
            rnd.GetBytes(data);

            return Convert.ToBase64String(data);
        }

        public string GenerateBase64UrlSafe(int dataLength = 32)
        {
            if (dataLength < 0)
                throw new InvalidDataLengthException(dataLength);

            if (dataLength == 0)
                return string.Empty;

            byte[] data = new byte[dataLength];
            using var rnd = RandomNumberGenerator.Create();
            rnd.GetBytes(data);

            return WebEncoders.Base64UrlEncode(data);
        }
    }
}
