using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GlucoPilot.Identity.Services;

internal sealed class TokenService : ITokenService
{
    private readonly IdentityOptions _options;
    private readonly IRepository<User> _userRepository;

    public TokenService(IOptions<IdentityOptions> options, IRepository<User> userRepository)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public string GenerateJwtToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_options.TokenSigningKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(GetClaims(user)),
            Expires = DateTime.UtcNow.AddMinutes(_options.TokenExpirationInMinutes),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(string ipAddress)
    {
        var now = DateTimeOffset.UtcNow;
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
            Expires = now.AddDays(_options.RefreshTokenExpirationInDays),
            Created = now,
            CreatedByIp = ipAddress,
        };

        var tokenIsUnique = !_userRepository.Any(u => u.RefreshTokens.Any(t => t.Token == refreshToken.Token));
        if (!tokenIsUnique)
        {
            return GenerateRefreshToken(ipAddress);
        }

        return refreshToken;
    }

    private static IEnumerable<Claim> GetClaims(User user)
    {
        yield return new Claim(ClaimTypes.NameIdentifier, user.Id.ToString());
        yield return new Claim(ClaimTypes.Email, user.Email);
    }
}