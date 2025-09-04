using FluentValidation;
using GlucoPilot.Api.Models;
using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Enums;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using GlucoPilot.LibreLinkClient;
using GlucoPilot.LibreLinkClient.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Endpoints.LibreLink.Login;

internal static class Endpoint
{
    internal static async Task<Results<Ok<LoginResponse>, ValidationProblem>> HandleAsync(
        [FromBody] LoginRequest request,
        [FromServices] IValidator<LoginRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] ILibreLinkClient libreLinkClient,
        [FromServices] IRepository<Patient> patientRepository,
        CancellationToken cancellationToken)
    {
        if (await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false) is
            { IsValid: false } validation)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        var userId = currentUser.GetUserId();

        try
        {
            var patient = await patientRepository
                .FindOneAsync(p => p.Id == userId, new FindOptions { IsAsNoTracking = false }, cancellationToken)
                .ConfigureAwait(false);
            if (patient is null)
            {
                throw new UnauthorizedException("PATIENT_NOT_FOUND");
            }

            if (patient.AuthTicket is not null &&
                DateTimeOffset.FromUnixTimeSeconds(patient.AuthTicket.Expires) > DateTimeOffset.UtcNow)
            {
                return TypedResults.Ok(new LoginResponse
                {
                    Token = patient.AuthTicket.Token,
                    Expires = patient.AuthTicket.Expires,
                    Duration = patient.AuthTicket.Duration,
                });
            }

            var authTicket = await libreLinkClient.LoginAsync(request.Username, request.Password, cancellationToken)
                .ConfigureAwait(false);

            if (patient.AuthTicket is null || patient.AuthTicket.Token != authTicket.Token)
            {
                patient.AuthTicket = new AuthTicket
                {
                    Token = authTicket.Token,
                    Expires = authTicket.Expires,
                    Duration = authTicket.Duration,
                };
                patient.GlucoseProvider = GlucoseProvider.LibreLink;
                await patientRepository.UpdateAsync(patient, cancellationToken).ConfigureAwait(false);
            }

            var response = new LoginResponse
            {
                Token = authTicket.Token,
                Expires = authTicket.Expires,
                Duration = authTicket.Duration,
            };

            return TypedResults.Ok(response);
        }
        catch (LibreLinkAuthenticationFailedException)
        {
            throw new UnauthorizedException("LIBRE_LINK_AUTH_FAILED");
        }
    }
}