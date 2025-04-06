using System;
using System.Threading;
using System.Threading.Tasks;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Models;
using static BCrypt.Net.BCrypt;

namespace GlucoPilot.Identity.Services;

public sealed class UserService : IUserService
{
    private readonly IRepository<User> _repository;
    private readonly ITokenService _tokenService;

    public UserService(IRepository<User> repository, ITokenService tokenService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _repository.FindOneAsync(u => u.Email == request.Email,
            new FindOptions { IsAsNoTracking = true, IsIgnoreAutoIncludes = true },
            cancellationToken: cancellationToken).ConfigureAwait(false);

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
        if (await _repository.AnyAsync(x => x.Email == request.Email, cancellationToken).ConfigureAwait(false))
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

            await _repository.AddAsync(patient, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var patient = await _repository.FindOneAsync(x => x.Id == request.PatientId.Value,
                    new FindOptions { IsAsNoTracking = true, IsIgnoreAutoIncludes = true }, cancellationToken)
                .ConfigureAwait(false);

            if (patient is not Patient patientEntity)
            {
                throw new NotFoundException("Patient_Not_Found");
            }

            var careGiver = new CareGiver
            {
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                AcceptedTerms = request.AcceptedTerms,
                Patients = [patientEntity],
            };
            user = careGiver;

            await _repository.AddAsync(careGiver, cancellationToken).ConfigureAwait(false);
        }

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