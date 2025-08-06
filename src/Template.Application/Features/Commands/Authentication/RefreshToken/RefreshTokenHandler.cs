using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using Cayd.AspNetCore.Mediator.Abstractions;
using System.Security.Claims;
using Template.Application.Abstractions.Authentication;
using Template.Application.Abstractions.Crypto;
using Template.Application.Abstractions.Http;
using Template.Application.Abstractions.UOW;
using Template.Application.Localization;
using Template.Application.Policies;

namespace Template.Application.Features.Commands.Authentication.RefreshToken
{
    public class RefreshTokenHandler : IAsyncHandler<RefreshTokenRequest, ExecResult<RefreshTokenResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRequestContext _requestContext;
        private readonly IHashing _hashing;
        private readonly IJwt _jwt;

        public RefreshTokenHandler(IUnitOfWork unitOfWork,
            IRequestContext requestContext,
            IHashing hashing,
            IJwt jwt)
        {
            _unitOfWork = unitOfWork;
            _requestContext = requestContext;
            _hashing = hashing;
            _jwt = jwt;
        }

        public async Task<ExecResult<RefreshTokenResponse>> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            if (_requestContext.UserId == null || 
                _requestContext.IsEmailVerified == null || 
                _requestContext.RefreshToken == null)
                return new ExecUnauthorized("The user is not logged in", AuthenticationLocalizationKeys.RefreshTokenNotLoggedIn);

            var hashedRefreshToken = _hashing.HashSha256(_requestContext.RefreshToken);
            var login = await _unitOfWork.Logins.GetByUserIdAndHashedRefreshTokenAsync(_requestContext.UserId.Value, hashedRefreshToken);
            if (login == null)
                return new ExecUnauthorized("The user is not logged in", AuthenticationLocalizationKeys.RefreshTokenNotLoggedIn);

            // Expiration date check
            if (DateTime.UtcNow > login.ExpirationDate)
            {
                _unitOfWork.Logins.Delete(login);

                await _unitOfWork.SaveChangesAsync();

                return new ExecGone("Token is expired", AuthenticationLocalizationKeys.TokenExpired);
            }

            // Generate JWT token
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, _requestContext.UserId.Value.ToString()),
                new Claim(EmailVerificationPolicy.ClaimName, _requestContext.IsEmailVerified.Value.ToString())
                // NOTE: Add more claims if needed
            };

            var userRoles = await _unitOfWork.Users.GetRolesByIdAsync(_requestContext.UserId.Value);
            if (userRoles != null && userRoles.Count != 0)
            {
                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }
            }

            var jwtToken = _jwt.GenerateJwtToken(claims);

            // Update login data
            login.RefreshTokenHashed = _hashing.HashSha256(jwtToken.RefreshToken);
            login.ExpirationDate = jwtToken.RefreshTokenExpirationDate;
            login.IpAddress = _requestContext.IpAddress;
            login.DeviceInfo = _requestContext.DeviceInfo;

            await _unitOfWork.SaveChangesAsync();

            return new RefreshTokenResponse()
            {
                AccessToken = jwtToken.AccessToken,
                RefreshToken = jwtToken.RefreshToken,
                RefreshTokenExpirationDate = jwtToken.RefreshTokenExpirationDate
            };
        }
    }
}
