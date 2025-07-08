namespace Template.Application.Dtos.Crypto.Enums
{
    public enum EPasswordVerificationResult
    {
        Fail                    =   0,
        VersionNotFound         =   1,
        LengthMismatch          =   2,
        Success                 =   3,
        SuccessRehashNeeded     =   4
    }
}
