using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.Identity.Models;

namespace GlucoPilot.Identity.Services;

public interface IUserService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, string origin, CancellationToken cancellationToken = default);
    Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);
}