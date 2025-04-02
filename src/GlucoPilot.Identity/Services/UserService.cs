using System;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data;
using GlucoPilot.Data.Entities;
using GlucoPilot.Identity.Models;
using Microsoft.EntityFrameworkCore;
using static BCrypt.Net.BCrypt;

namespace GlucoPilot.Identity.Services;

public sealed class UserService : IUserService
{
    private readonly GlucoPilotDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public UserService(GlucoPilotDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken)
            .ConfigureAwait(false) ?? await _dbContext.Patients
            .FirstOrDefaultAsync(p => p.Email == request.Email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null || !Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Username_Or_Password_Is_Incorrect");
        }

        var token = _tokenService.GenerateJwtToken(user);
        var response = new LoginResponse
        {
            Token = token,
        };
        return response;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Users.AnyAsync(x => x.Email == request.Email, cancellationToken).ConfigureAwait(false) ||
            await _dbContext.Patients.AnyAsync(x => x.Email == request.Email, cancellationToken).ConfigureAwait(false))
        {
            throw new ConflictException("User_Already_Exists");
        }

        User user;
        if (request.PatientId is null)
        {
            var patient = new Patient
            {
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                AcceptedTerms = request.AcceptedTerms,
            };
            user = patient;

            await _dbContext.Patients.AddAsync(patient, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var patient = await _dbContext.Patients
                .FirstOrDefaultAsync(x => x.Id == request.PatientId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (patient is null)
            {
                throw new NotFoundException("Patient_Not_Found");
            }
            var careGiver = new CareGiver
            {
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                AcceptedTerms = request.AcceptedTerms,
                Patients = [patient],
            };
            user = careGiver;

            await _dbContext.Users.AddAsync(careGiver, cancellationToken).ConfigureAwait(false);
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email,
            Created = user.Created,
            Updated = user.Updated,
            AcceptedTerms = user.AcceptedTerms,
        };
    }
}