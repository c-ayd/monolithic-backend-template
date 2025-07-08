namespace Template.Application.Abstractions.Crypto
{
    public interface ITokenGenerator
    {
        string Generate(int dataLength = 32);
        string GenerateBase64UrlSafe(int dataLength = 32);
    }
}
