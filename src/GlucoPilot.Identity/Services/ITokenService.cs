using GlucoPilot.Data.Entities;

namespace GlucoPilot.Identity.Services;

public interface ITokenService
{
    string GenerateJwtToken(User user);
    RefreshToken GenerateRefreshToken(string ipAddress);
}