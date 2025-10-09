using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using GlucoPilot.LibreLinkClient.Exceptions;
using GlucoPilot.LibreLinkClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using AuthTicket = GlucoPilot.LibreLinkClient.Models.AuthTicket;

namespace GlucoPilot.Api.Endpoints.LibreLink.PairConnection;

internal static class Endpoint
{
    internal static async Task<Results<Ok<PairConnectionResponse>, UnauthorizedHttpResult, NotFound, ValidationProblem>>
        HandleAsync(
            [FromBody] PairConnectionRequest request,
            [FromServices] ICurrentUser currentUser,
            [FromServices] IRepository<Patient> patientRepository,
            [FromServices] ILibreLinkClient libreLinkClient,
            CancellationToken cancellationToken)
    {
        var userId = currentUser.GetUserId();

        try
        {
            var patient = await patientRepository
                .FindOneAsync(p => p.Id == userId, new FindOptions { IsAsNoTracking = false }, cancellationToken)
                .ConfigureAwait(false);
            if (patient is null || string.IsNullOrWhiteSpace(patient.AuthTicket?.Token))
            {
                throw new UnauthorizedException("PATIENT_NOT_FOUND");
            }

            await libreLinkClient
                .LoginAsync(
                    new AuthTicket
                    {
                        Token = patient.AuthTicket.Token,
                        Expires = patient.AuthTicket.Expires,
                        Duration = patient.AuthTicket.Duration,
                        PatientId = patient.AuthTicket.PatientId
                    }, cancellationToken).ConfigureAwait(false);

            var connections = await libreLinkClient.GetConnectionsAsync(cancellationToken).ConfigureAwait(false);
            var connection = connections.FirstOrDefault(c => c.PatientId == request.PatientId);

            if (connection is null)
            {
                throw new NotFoundException("CONNECTION_NOT_FOUND");
            }

            patient.PatientId = request.PatientId.ToString();

            await patientRepository.UpdateAsync(patient, cancellationToken);

            var response = new PairConnectionResponse
            {
                Id = patient.Id,
                PatientId = Guid.Parse(patient.PatientId),
                FirstName = connection.FirstName,
                LastName = connection.LastName,
            };

            return TypedResults.Ok(response);
        }
        catch (LibreLinkNotAuthenticatedException)
        {
            throw new UnauthorizedException("LIBRE_LINK_NOT_AUTHENTICATED");
        }
        catch (LibreLinkAuthenticationExpiredException)
        {
            throw new UnauthorizedException("LIBRE_LINK_AUTH_EXPIRED");
        }
        catch (LibreLinkAuthenticationFailedException)
        {
            throw new UnauthorizedException("LIBRE_LINK_AUTH_FAILED");
        }
    }
}