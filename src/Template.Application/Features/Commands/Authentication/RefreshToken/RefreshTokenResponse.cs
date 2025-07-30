namespace Template.Application.Features.Commands.Authentication.RefreshToken
{
    public class RefreshTokenResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required DateTime RefreshTokenExpirationDate { get; set; }
    }
}
