using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using Cayd.AspNetCore.ExecutionResult.ServerError;
using Cayd.AspNetCore.FlexLog;
using MediatR;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Template.Application.Abstractions.Authentication;
using Template.Application.Abstractions.Crypto;
using Template.Application.Abstractions.Http;
using Template.Application.Abstractions.UOW;
using Template.Application.Dtos.Crypto.Enums;
using Template.Application.Localization;
using Template.Application.Policies;
using Template.Application.Settings;

namespace Template.Application.Features.Commands.Authentication.Login
{
    public class LoginHandler : IRequestHandler<LoginRequest, ExecResult<LoginResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHashing _hashing;
        private readonly IJwt _jwt;
        private readonly IRequestContext _requestContext;
        private readonly AccountLockSettings _accountLockSettings;
        private readonly IFlexLogger<LoginHandler> _flexLogger;

        public LoginHandler(IUnitOfWork unitOfWork,
            IHashing hashing,
            IJwt jwt,
            IRequestContext requestContext,
            IOptions<AccountLockSettings> accountLockSettings,
            IFlexLogger<LoginHandler> flexLogger)
        {
            _unitOfWork = unitOfWork;
            _hashing = hashing;
            _jwt = jwt;
            _requestContext = requestContext;
            _accountLockSettings = accountLockSettings.Value;
            _flexLogger = flexLogger;
        }

        public async Task<ExecResult<LoginResponse>> Handle(LoginRequest request, CancellationToken cancellationToken)
        {
            var securityState = await _unitOfWork.Users.GetSecurityStateByEmailAsync(request.Email!);
            if (securityState == null)
                return new ExecErrorDetail("The credentials are wrong", AuthenticationLocalizationKeys.LoginWrongCredentials);

            // Account lock check
            if (securityState.IsLocked)
            {
                if (DateTime.UtcNow < securityState.UnlockDate)
                {
                    return new ExecLocked($"The account is locked until {securityState.UnlockDate}",
                        AuthenticationLocalizationKeys.LoginAlreadyLocked,
                        new
                        {
                            securityState.UnlockDate
                        });
                }
                else
                {
                    securityState.IsLocked = false;
                    securityState.FailedAttempts = 0;
                }
            }

            // Compare passwords
            var passwordVerificationResult = _hashing.VerifyPassword(securityState.PasswordHashed!, request.Password!);
            switch (passwordVerificationResult)
            {
                case EPasswordVerificationResult.Fail:
                    ++securityState.FailedAttempts;
                    if (securityState.FailedAttempts >= _accountLockSettings.FailedAttemptsForSecondLock)
                    {
                        securityState.FailedAttempts = 0;
                        securityState.IsLocked = true;
                        securityState.UnlockDate = DateTime.UtcNow.AddMinutes(_accountLockSettings.SecondLockTimeInMinutes);

                        _flexLogger.LogWarning("An account is locked.", new
                        {
                            securityState.UserId
                        });
                    }
                    else if (securityState.FailedAttempts >= _accountLockSettings.FailedAttemptsForFirstLock)
                    {
                        securityState.IsLocked = true;
                        securityState.UnlockDate = DateTime.UtcNow.AddMinutes(_accountLockSettings.FirstLockTimeInMinutes);
                    }
                    else
                    {
                        await _unitOfWork.SaveChangesAsync();

                        return new ExecErrorDetail("The credentials are wrong", AuthenticationLocalizationKeys.LoginWrongCredentials);
                    }

                    await _unitOfWork.SaveChangesAsync();

                    return new ExecLocked($"The account is locked due to {securityState.FailedAttempts} failed attemps",
                        AuthenticationLocalizationKeys.LoginLocked,
                        new
                        {
                            securityState.FailedAttempts,
                            securityState.UnlockDate
                        });
                case EPasswordVerificationResult.VersionNotFound:
                    _flexLogger.LogError("Password verification failed due to the version difference.", new
                    {
                        PasswordVersion = Convert.FromBase64String(securityState.PasswordHashed!)[0]
                    });
                    return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
                case EPasswordVerificationResult.LengthMismatch:
                    _flexLogger.LogError("Passwords' lengths do not match.", new
                    {
                        PasswordVersion = Convert.FromBase64String(securityState.PasswordHashed!)[0]
                    });
                    return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
                case EPasswordVerificationResult.SuccessRehashNeeded:
                    securityState.FailedAttempts = 0;
                    securityState.PasswordHashed = _hashing.HashPassword(request.Password!);
                    break;
                case EPasswordVerificationResult.Success:
                    securityState.FailedAttempts = 0;
                    break;
                default:
                    _flexLogger.LogError("Password verification result is outside of the enum range.", new
                    {
                        VerificationResult = passwordVerificationResult
                    });
                    return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
            }

            // Generate JWT token
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, securityState.UserId.ToString()),
                new Claim(EmailVerificationPolicy.ClaimName, securityState.IsEmailVerified.ToString())
                // NOTE: Add more claims if needed
            };

            var userRoles = await _unitOfWork.Users.GetRolesByIdAsync(securityState.UserId);
            if (userRoles != null && userRoles.Count != 0)
            {
                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }
            }

            var jwtToken = _jwt.GenerateJwtToken(claims);

            // Add a new login to the database
            await _unitOfWork.Logins.AddAsync(new Domain.Entities.UserManagement.Login()
            {
                RefreshTokenHashed = _hashing.HashSha256(jwtToken.RefreshToken),
                ExpirationDate = jwtToken.RefreshTokenExpirationDate,
                IpAddress = _requestContext.IpAddress,
                DeviceInfo = _requestContext.DeviceInfo,
                UserId = securityState.UserId
            });

            await _unitOfWork.SaveChangesAsync();

            return new LoginResponse()
            {
                AccessToken = jwtToken.AccessToken,
                RefreshToken = jwtToken.RefreshToken,
                RefreshTokenExpirationDate = jwtToken.RefreshTokenExpirationDate,
            };
        }
    }
}
