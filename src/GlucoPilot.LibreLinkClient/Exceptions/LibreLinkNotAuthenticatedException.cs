using System;

namespace GlucoPilot.LibreLinkClient.Exceptions;

public class LibreLinkNotAuthenticatedException()
    : Exception("LibreLink is not authenticated. Please authenticate before making requests.");