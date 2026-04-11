using System;

namespace GlucoPilot.LibreLinkClient.Exceptions;

public class LibreLinkRegionRedirectException(LibreRegion region) : Exception(
    $"Authentication requires region redirect to {region}.")
{
    public LibreRegion Region { get; } = region;
}