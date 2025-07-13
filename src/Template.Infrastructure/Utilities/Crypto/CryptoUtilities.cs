namespace Template.Infrastructure.Utilities.Crypto
{
    public static class CryptoUtilities
    {
        public static int GetDecodedBase64ByteCount(string base64str)
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
    }
}
