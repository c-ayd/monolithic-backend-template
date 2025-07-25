namespace Template.Application.Dtos.Authentication
{
    public class JwtTokenDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required DateTime RefreshTokenExpirationDate { get; set; }
    }
}
