namespace GlucoPilot.LibreLinkClient.Models;

public sealed record ConnectionResponse : LibreLinkResponse<IReadOnlyCollection<ConnectionData>>
{
}