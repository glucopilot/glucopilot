using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Data.Entities;
using GlucoPilot.Identity.Models;

namespace GlucoPilot.Identity.Services;

public interface IUserService
{
    Task<User> FindByRefreshTokenAsync(string? token, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, string origin, CancellationToken cancellationToken = default);
    Task<TokenResponse> RefreshTokenAsync(string? token, string ipAddress, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string token, string ipAddress, CancellationToken cancellationToken = default);
    Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);
}