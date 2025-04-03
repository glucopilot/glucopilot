# GlucoPilot.Identity
The GlucoPilot.Identity project is a .NET library that provides authentication/authorization services for the GlucoPilot application. It includes features such as user registration, login.

## Usage
To register the identity services:
```csharp
services.AddIdentity(builder.Configuration.GetSection("Identity").Bind);
```

To register the identity middleware:
```csharp
app.UseIdentity();
```

To map the identity endpoints:
```csharp
app.MapIdentityEndpoints();
```

## Features
- User registration
- User login

### User Registration
`api/v1/identity/register`

There are two ways to register a user:

#### As a Patient
When calling the register endpoint, a patient is distinguished by having now `PatientId` set in the `RegisterRequest`.

#### As a Care Giver
If the `PatientId` is set, the user will be registered as a `CareGiver` with basic access to the patient until authorized to access specific patient data.

### User Login
`api/v1/identity/register`

The user can log in using the `LoginRequest` endpoint. The user must provide their email and password. If the credentials are valid, a JWT token will be returned.

> Note: the intention here is to add a refresh token at a later date.