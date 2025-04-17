using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Models;
using GlucoPilot.Identity.Templates;
using GlucoPilot.Mail;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using static BCrypt.Net.BCrypt;

namespace GlucoPilot.Identity.Services;

public sealed class UserService : IUserService
{
    private readonly IRepository<User> _repository;
    private readonly ITokenService _tokenService;
    private readonly IMailService _mailService;
    private readonly ITemplateService _templateService;
    private readonly IdentityOptions _options;

    public UserService(IRepository<User> repository, ITokenService tokenService, IMailService mailService,
        ITemplateService templateService, IOptions<IdentityOptions> options)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _repository.FindOneAsync(u => u.Email == request.Email,
            new FindOptions { IsAsNoTracking = true, IsIgnoreAutoIncludes = true },
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (user is null || (_options.RequireEmailVerification && !user.IsVerified) ||
            !Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("EMAIL_OR_PASSWORD_INCORRECT");
        }

        var token = _tokenService.GenerateJwtToken(user);
        var response = new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            IsVerified = user.IsVerified,
            GlucoseProvider = user is Patient patient ? (GlucoseProvider)patient.GlucoseProvider : null,
        };
        return response;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, string origin,
        CancellationToken cancellationToken = default)
    {
        if (await _repository.AnyAsync(x => x.Email == request.Email, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("USER_ALREADY_EXISTS");
        }

        User user;
        if (request.PatientId is null)
        {
            var patient = new Patient
            {
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                AcceptedTerms = request.AcceptedTerms,
                EmailVerificationToken = _options.RequireEmailVerification
                    ? await GenerateVerificationTokenAsync(cancellationToken).ConfigureAwait(false)
                    : null,
                IsVerified = !_options.RequireEmailVerification,
                FirstName = request.FirstName,
                LastName = request.LastName,
            };
            user = patient;

            await _repository.AddAsync(patient, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var patient = await _repository.FindOneAsync(x => x.Id == request.PatientId.Value,
                    new FindOptions { IsAsNoTracking = true, IsIgnoreAutoIncludes = true }, cancellationToken)
                .ConfigureAwait(false);

            if (patient is not Patient patientEntity)
            {
                throw new NotFoundException("PATIENT_NOT_FOUND");
            }

            var careGiver = new CareGiver
            {
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                AcceptedTerms = request.AcceptedTerms,
                Patients = [patientEntity],
                EmailVerificationToken = _options.RequireEmailVerification
                    ? await GenerateVerificationTokenAsync(cancellationToken).ConfigureAwait(false)
                    : null,
                IsVerified = !_options.RequireEmailVerification,
                FirstName = request.FirstName,
                LastName = request.LastName,
            };
            user = careGiver;

            await _repository.AddAsync(careGiver, cancellationToken).ConfigureAwait(false);
        }

        if (_options.RequireEmailVerification)
        {
            var confirmEmailModel = new EmailConfirmation()
            {
                Email = user.Email,
                Url = GetEmailVerificationUrl(user.EmailVerificationToken!, origin),
            };

            var message = new MailRequest()
            {
                To = [user.Email],
                Subject = "Verify your email",
                Body = await _templateService
                    .RenderTemplateAsync("EmailConfirmation", confirmEmailModel, cancellationToken)
                    .ConfigureAwait(false),
            };
            await _mailService.SendAsync(message, cancellationToken).ConfigureAwait(false);
        }

        return new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email,
            Created = user.Created,
            Updated = user.Updated,
            AcceptedTerms = user.AcceptedTerms,
            EmailVerified = !_options.RequireEmailVerification,
        };
    }

    public async Task<TokenResponse> RefreshTokenAsync(string? token, string ipAddress, CancellationToken cancellationToken)
    {
        var user = await FindByRefreshTokenAsync(token, cancellationToken).ConfigureAwait(false);
        var refreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == token);

        if (refreshToken?.IsRevoked ?? false)
        {
            RevokeRefreshTokensRecursively(refreshToken, user, ipAddress,
                $"Attempted use of revoked ancestor token: {token}");
            await _repository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        }

        if (!(refreshToken?.IsActive ?? false))
        {
            throw new UnauthorizedException("REFRESH_TOKEN_INCORRECT");
        }

        var newRefreshToken = _tokenService.GenerateRefreshToken(ipAddress);
        RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        user.RefreshTokens.Add(newRefreshToken);

        RemoveOldRefreshTokens(user);

        await _repository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);

        var jwtToken = _tokenService.GenerateJwtToken(user);
        var response = new TokenResponse()
        {
            Token = jwtToken,
            RefreshToken = newRefreshToken.Token,
        };

        return response;
    }

    public async Task<User> FindByRefreshTokenAsync(string? token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedException("INVALID_TOKEN");
        }

        var user = await _repository.FindOneAsync(u => u.RefreshTokens.Any(t => t.Token == token), new FindOptions { IsAsNoTracking = false }, cancellationToken)
            .ConfigureAwait(false);
        if (user is null)
        {
            throw new UnauthorizedException("INVALID_TOKEN");
        }

        return user;
    }

    public async Task RevokeTokenAsync(string token, string ipAddress, CancellationToken cancellationToken)
    {
        var user = await FindByRefreshTokenAsync(token, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            throw new UnauthorizedException("INVALID_TOKEN");
        }
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
        {
            throw new UnauthorizedException("INVALID_TOKEN");
        }

        RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
        await _repository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
    }

    private static void RevokeRefreshTokensRecursively(RefreshToken refreshToken, User user, string ipAddress, string reason)
    {
        if (string.IsNullOrEmpty(refreshToken.ReplacedByToken))
        {
            return;
        }

        var child = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
        if (child is null)
        {
            return;
        }

        if (child.IsActive)
        {
            RevokeRefreshToken(child, ipAddress, reason);
        }
        else
        {
            RevokeRefreshTokensRecursively(child, user, ipAddress, reason);
        }
    }

    private static void RevokeRefreshToken(RefreshToken refreshToken, string ipAddress, string? reason = null, string? replacedByToken = null)
    {
        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.RevokedReason = reason;
        refreshToken.ReplacedByToken = replacedByToken;
    }

    private async Task<string> GenerateVerificationTokenAsync(CancellationToken cancellationToken)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        var tokenIsUnique = !await _repository.AnyAsync(x => x.EmailVerificationToken == token, cancellationToken)
            .ConfigureAwait(false);
        if (!tokenIsUnique)
        {
            return await GenerateVerificationTokenAsync(cancellationToken).ConfigureAwait(false);
        }

        return token;
    }

    private static string GetEmailVerificationUrl(string verificationToken, string origin)
    {
        const string route = "api/v1/identity/verify-email";
        var endpointUri = new Uri(string.Concat($"{origin}/", route));
        return QueryHelpers.AddQueryString(endpointUri.ToString(), "token", verificationToken);
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _repository.FindOneAsync(x => x.EmailVerificationToken == request.Token,
                new FindOptions() { IsAsNoTracking = true, IsIgnoreAutoIncludes = true }, cancellationToken)
            .ConfigureAwait(false);
        if (user is null)
        {
            throw new UnauthorizedException("EMAIL_VERIFICATION_TOKEN_INVALID");
        }

        user.EmailVerificationToken = null;
        user.IsVerified = true;

        await _repository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
    }

    private void RemoveOldRefreshTokens(User user)
    {
        user.RefreshTokens.RemoveAll(x =>
            !x.IsActive && x.Created.AddDays(_options.RefreshTokenExpirationInDays) <= DateTime.UtcNow);
    }
}