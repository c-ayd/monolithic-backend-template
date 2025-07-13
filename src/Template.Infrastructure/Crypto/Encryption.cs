using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Template.Application.Abstractions.Crypto;
using Template.Infrastructure.Settings.Crypto;
using Template.Infrastructure.Utilities.Crypto;

namespace Template.Infrastructure.Crypto
{
    public class Encryption : IEncryption
    {
        public byte TypeId { get; private set; } = 1;

        private static readonly int _typeSize = sizeof(byte);

        private static readonly int _nonceSize = AesGcm.NonceByteSizes.MaxSize;
        private static readonly int _tagSize = AesCcm.TagByteSizes.MaxSize;

        private readonly AesSettings _aesSettings;

        public Encryption(IOptions<AesSettings> aesSettings)
        {
            _aesSettings = aesSettings.Value;
        }

        public string Encrypt(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("The value cannot be null or empty.", nameof(value));

            byte[] valueBytes = Encoding.UTF8.GetBytes(value);

            int encryptedValueByteCount =
                _typeSize +
                _nonceSize +
                _tagSize +
                valueBytes.Length;

            Span<byte> encryptedValueBytes = new byte[encryptedValueByteCount];
            encryptedValueBytes[0] = TypeId;

            var nonceSpan = encryptedValueBytes.Slice(_typeSize, _nonceSize);
            var tagSpan = encryptedValueBytes.Slice(_typeSize + _nonceSize, _tagSize);
            var cipherSpan = encryptedValueBytes.Slice(_typeSize + _nonceSize + _tagSize, valueBytes.Length);

            RandomNumberGenerator.Fill(nonceSpan);
            var keyBytes = Encoding.UTF8.GetBytes(_aesSettings.Key);
            using (var aesGcm = new AesGcm(keyBytes, _tagSize))
            {
                aesGcm.Encrypt(nonceSpan, valueBytes, cipherSpan, tagSpan);
            }

            return Convert.ToBase64String(encryptedValueBytes.Slice(0, _typeSize + _nonceSize + _tagSize + valueBytes.Length));
        }

        public string? Decrypt(string encryptedValue)
        {
            if (string.IsNullOrEmpty(encryptedValue))
                throw new ArgumentException($"The encrypted value cannot be null or empty.", nameof(encryptedValue));

            var encryptedValueByteCount = CryptoUtilities.GetDecodedBase64ByteCount(encryptedValue);
            var cipherSize = encryptedValueByteCount - (_typeSize + _nonceSize + _tagSize);

            Span<byte> encryptedValueBytes = new byte[encryptedValueByteCount + cipherSize + cipherSize];

            if (!Convert.TryFromBase64String(encryptedValue, encryptedValueBytes, out _))
                return null;

            var typeId = encryptedValueBytes[0];
            if (typeId != TypeId)
                return null;

            var nonceSpan = encryptedValueBytes.Slice(_typeSize, _nonceSize);
            var tagSpan = encryptedValueBytes.Slice(_typeSize + _nonceSize, _tagSize);
            var cipherSpan = encryptedValueBytes.Slice(_typeSize + _nonceSize + _tagSize, cipherSize);
            var valueSpan = encryptedValueBytes.Slice(_typeSize + _nonceSize + _tagSize + cipherSize, cipherSize);

            var keyBytes = Encoding.UTF8.GetBytes(_aesSettings.Key);
            using (var aesGcm = new AesGcm(keyBytes, _tagSize))
            {
                aesGcm.Decrypt(nonceSpan, cipherSpan, tagSpan, valueSpan);
            }

            return Encoding.UTF8.GetString(valueSpan);
        }

        public bool Compare(string encryptedValue, string value)
        {
            if (string.IsNullOrEmpty(encryptedValue))
                throw new ArgumentException($"The encrypted value cannot be null or empty.", nameof(encryptedValue));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("The value cannot be null or empty.", nameof(value));

            var decryptedValue = Decrypt(encryptedValue);
            if (decryptedValue == null)
                return false;

            var decryptedValueBytes = Encoding.UTF8.GetBytes(decryptedValue);
            var valueBytes = Encoding.UTF8.GetBytes(value);
            return CryptographicOperations.FixedTimeEquals(decryptedValueBytes, valueBytes);
        }
    }
}
