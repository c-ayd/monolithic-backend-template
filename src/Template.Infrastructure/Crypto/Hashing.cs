using System.Security.Cryptography;
using System.Text;
using Template.Application.Abstractions.Crypto;
using Template.Application.Dtos.Crypto.Enums;
using Template.Infrastructure.Crypto.Structs;

namespace Template.Infrastructure.Crypto
{
    public class Hashing : IHashing
    {
        private static readonly int _versionSize = sizeof(byte);

        private static readonly byte _currentVersion = 1;
        private static readonly Dictionary<byte, HashingVersion> _hashingVersions = new Dictionary<byte, HashingVersion>()
        {
            { 1, new HashingVersion(HashAlgorithmName.SHA256, saltSize: 256 / 8, keySize: 256 / 8, iterations: 600000) }
        };

        private static readonly HashingVersion _currentHashing = _hashingVersions[_currentVersion];

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("The password cannot be null or empty.", nameof(password));

            var hashedPasswordByteCount = _versionSize + _currentHashing.SaltSize + _currentHashing.KeySize;
            Span<byte> hashedPasswordBytes = new byte[hashedPasswordByteCount];
            var salyBytes = hashedPasswordBytes.Slice(_versionSize, _currentHashing.SaltSize);
            var keyBytes = hashedPasswordBytes.Slice(_versionSize + _currentHashing.SaltSize, _currentHashing.KeySize);

            hashedPasswordBytes[0] = _currentVersion;
            RandomNumberGenerator.Fill(salyBytes);
            Rfc2898DeriveBytes.Pbkdf2(password, salyBytes, keyBytes, _currentHashing.Iterations, _currentHashing.Algorithm);

            return Convert.ToBase64String(hashedPasswordBytes);
        }

        public EPasswordVerificationResult VerifyPassword(string hashedPassword, string password)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                throw new ArgumentException("The hashed password cannot be null or empty.", nameof(hashedPassword));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("The password cannot be null or empty.", nameof(password));

            var hashedPasswordByteCount = GetDecodedBase64ByteCount(hashedPassword);
            Span<byte> hashedPasswordBytes = new byte[hashedPasswordByteCount];
            if (!Convert.TryFromBase64String(hashedPassword, hashedPasswordBytes, out _))
                return EPasswordVerificationResult.Fail;

            var version = hashedPasswordBytes[0];
            if (!_hashingVersions.TryGetValue(version, out var _hashing))
                return EPasswordVerificationResult.VersionNotFound;

            var expectedHashedPasswordCount = _versionSize + _hashing.SaltSize + _hashing.KeySize;
            if (hashedPasswordByteCount != expectedHashedPasswordCount)
                return EPasswordVerificationResult.LengthMismatch;

            var saltBytes = hashedPasswordBytes.Slice(_versionSize, _hashing.SaltSize);
            var keyBytes = hashedPasswordBytes.Slice(_versionSize + _hashing.SaltSize, _hashing.KeySize);

            Span<byte> providedKeyBytes = new byte[_hashing.KeySize];
            Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, providedKeyBytes, _hashing.Iterations, _hashing.Algorithm);

            if (!CryptographicOperations.FixedTimeEquals(keyBytes, providedKeyBytes))
                return EPasswordVerificationResult.Fail;

            return version == _currentVersion ? 
                EPasswordVerificationResult.Success :
                EPasswordVerificationResult.SuccessRehashNeeded;
        }

        private int GetDecodedBase64ByteCount(string base64str)
        {
            var characterCount = base64str.Length;
            var paddingCount = 0;

            if (characterCount > 0)
            {
                if (base64str[characterCount - 1] == '=')
                {
                    ++paddingCount;

                    if (characterCount > 1 && base64str[characterCount - 2] == '=')
                    {
                        ++paddingCount;
                    }
                }
            }

            return ((characterCount * 3) / 4) - paddingCount;
        }

        public string HashSha256(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("The value cannot be null or empty.", nameof(value));

            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            byte[] hashedValueBytes = SHA256.HashData(valueBytes);
            return Convert.ToBase64String(hashedValueBytes);
        }

        public bool CompareSha256(string hashedValue, string value)
        {
            if (string.IsNullOrEmpty(hashedValue))
                throw new ArgumentException($"The hashed value cannot be null or empty.", nameof(hashedValue));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"The value cannot be null or empty.", nameof(value));

            byte[] hashedValueBytes = Convert.FromBase64String(hashedValue);

            byte[] providedValueBytes = Encoding.UTF8.GetBytes(value);
            byte[] providedHashedValueBytes = SHA256.HashData(providedValueBytes);

            return CryptographicOperations.FixedTimeEquals(hashedValueBytes, providedHashedValueBytes);
        }
    }
}
