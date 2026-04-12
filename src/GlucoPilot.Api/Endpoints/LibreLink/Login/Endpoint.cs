using FluentValidation;
using GlucoPilot.AspNetCore.Exceptions;
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
using GlucoPilot.Api.Extensions;
using GlucoPilot.Data.Entities;
using AuthTicket = GlucoPilot.LibreLinkClient.Models.AuthTicket;

namespace GlucoPilot.Api.Endpoints.LibreLink.Login;

internal static class Endpoint
{
    internal static async Task<Results<Ok<LoginResponse>, ValidationProblem>> HandleAsync(
        [FromBody] LoginRequest request,
        [FromServices] IValidator<LoginRequest> validator,
        [FromServices] ICurrentUser currentUser,
        [FromServices] ILibreLinkClientFactory libreLinkClientFactory,
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

            if (patient.AuthTicket is not null && !string.IsNullOrWhiteSpace(patient.AuthTicket.PatientId) &&
                DateTimeOffset.FromUnixTimeSeconds(patient.AuthTicket.Expires) > DateTimeOffset.UtcNow)
            {
                return TypedResults.Ok(new LoginResponse
                {
                    Token = patient.AuthTicket.Token,
                    Expires = patient.AuthTicket.Expires,
                    Duration = patient.AuthTicket.Duration,
                });
            }

            if (!patient.Region.HasValue)
            {
                patient.Region = Region.Eu;
            }

            var libreLinkClient = libreLinkClientFactory.CreateLibreLinkClient(patient.Region.Value.ToLibreRegion());

            AuthTicket? authTicket;
            try
            {
                authTicket = await libreLinkClient.LoginAsync(request.Username, request.Password, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (LibreLinkRegionRedirectException ex)
            {
                patient.Region = ex.Region.ToRegion();
                libreLinkClient = libreLinkClientFactory.CreateLibreLinkClient(ex.Region);
                authTicket = await libreLinkClient.LoginAsync(request.Username, request.Password, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (patient.AuthTicket is null || patient.AuthTicket.Token != authTicket.Token)
            {
                patient.AuthTicket = new Data.Entities.AuthTicket
                {
                    Token = authTicket.Token,
                    Expires = authTicket.Expires,
                    Duration = authTicket.Duration,
                    PatientId = authTicket.PatientId ?? string.Empty,
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