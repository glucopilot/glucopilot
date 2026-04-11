namespace GlucoPilot.LibreLinkClient;

public interface ILibreLinkClientFactory
{
    ILibreLinkClient CreateLibreLinkClient(LibreRegion region);
}