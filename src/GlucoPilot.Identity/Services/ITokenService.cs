using GlucoPilot.Data.Entities;

namespace GlucoPilot.Identity.Services;

internal interface ITokenService
{
    string GenerateJwtToken(User user);
}