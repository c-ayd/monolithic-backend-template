namespace Template.Application.Dtos.Authentication
{
    public class JwtTokenDto
    {
        public required string AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpirationDate { get; set; }
    }
}
