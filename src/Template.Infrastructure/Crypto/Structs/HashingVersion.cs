using System.Security.Cryptography;

namespace Template.Infrastructure.Crypto.Structs
{
    public struct HashingVersion
    {
        public HashAlgorithmName Algorithm { get; }
        public int SaltSize { get; }
        public int KeySize { get; }
        public int Iterations { get; }

        public HashingVersion(HashAlgorithmName algorithm, int saltSize, int keySize, int iterations)
        {
            Algorithm = algorithm;
            SaltSize = saltSize;
            KeySize = keySize;
            Iterations = iterations;
        }
    }
}
