namespace Template.Application.Abstractions.Crypto
{
    public interface IEncryption
    {
        byte TypeId { get; }

        string Encrypt(string value);
        string? Decrypt(string encryptedValue);
        bool Compare(string encryptedValue, string value);
    }
}
