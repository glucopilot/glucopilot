# GlucoPilot.LibreLinkClient
The C# client library for accessing LibreLink data.

## Usage
Register the client with:

```csharp
// Add the LibreLinkClient and required services to DI.
services.AddLibreLinkClient(configuration.GetSection("LibreLink"));
```

Before making any requests to LibreLink, you must first authenticate the client. Failure to authenticate before a request will result in a `LibreLinkNotAuthenticatedException` exception. 

Authentication is maintained for the lifetime of the client and when authentication expires, a `LibreLinkAuthenticationExpiredException` is thrown when attempting to call the client.

There are two methods of authenticating:

### Username and Password
When using a username and password, the request is sent to LibreLink to authenticate and an auth ticket is returned.

```csharp
// Authenticate with username and password.
_ = await libreLinkClient.LoginAsync("username", "password");
```

This auth ticket should then be stored and re-used with the auth ticket authentication method so you do not need to continually ask for a username and password.

### Auth Ticket
Once authenticated with a username and password, the client returns an auth ticket which can be stored and re-used to authenticate the client.

```csharp
// Authenticate with username and password.
var authTicket = await libreLinkClient.LoginAsync("username", "password");
// Store the auth ticket somewhere.
store.SaveTicket("username", authTicket);

// ...

// Load the stored auth ticket.
var authTicket = store.LoadTicket("username");
// Authenticate with the previous auth ticket.
await libreLinkClient.LoginAsync(authTicket);
```

If the auth ticket used to authenticate has expired, a `LibreLinkAuthenticationExpiredException` will be thrown.

> _Note: The auth ticket contains a long-lived access token that enables users to access LibreLink data. This data should be treated a sensitive and stored as such._