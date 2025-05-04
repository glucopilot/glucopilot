using GlucoPilot.AspNetCore.Exceptions;
using GlucoPilot.Data.Entities;
using GlucoPilot.Data.Repository;
using GlucoPilot.Identity.Authentication;
using GlucoPilot.LibreLinkClient;
using GlucoPilot.LibreLinkClient.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibreAuthTicket = GlucoPilot.LibreLinkClient.Models.AuthTicket;

namespace GlucoPilot.Api.Endpoints.LibreLink.Connections;

internal static class Endpoint
{
    internal static async Task<Ok<List<ConnectionResponse>>> HandleAsync(
        [FromServices] ICurrentUser currentUser,
        [FromServices] ILibreLinkClient libreLinkClient,
        [FromServices] IRepository<Patient> patientRepository,
        CancellationToken cancellationToken)
    {
        var patient = patientRepository.FindOne(p => p.Id == currentUser.GetUserId());
        if (patient is null || patient.AuthTicket is null)
        {
            throw new UnauthorizedException("PATIENT_NOT_FOUND");
        }

        var authTicket = new LibreAuthTicket
        {
            Token = patient.AuthTicket.Token,
            Expires = patient.AuthTicket.Expires
        };
        try
        {
            await libreLinkClient.LoginAsync(authTicket, cancellationToken).ConfigureAwait(false);

            var connections = await libreLinkClient.GetConnectionsAsync(cancellationToken).ConfigureAwait(false);

            if (connections is null)
            {
                return TypedResults.Ok(new List<ConnectionResponse>());
            }

            var response = connections.Select(c => new ConnectionResponse
            {
                PatientId = c.PatientId,
                FirstName = c.FirstName,
                LastName = c.LastName,
            }).ToList();

            return TypedResults.Ok(response);
        }
        catch (LibreLinkAuthenticationExpiredException)
        {
            throw new UnauthorizedException("LIBRE_LINK_AUTH_EXPIRED");
        }
        catch (LibreLinkNotAuthenticatedException)
        {
            throw new UnauthorizedException("LIBRE_LINK_NOT_AUTHENTICATED");
        }
    }
}
