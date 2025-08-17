FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN addgroup --system appgroup && adduser --system appuser --ingroup appgroup
RUN chown -R appuser:appgroup /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["GlucoPilot.Api/GlucoPilot.Api.csproj", "GlucoPilot.Api/"]
RUN dotnet restore "GlucoPilot.Api/GlucoPilot.Api.csproj"
COPY ["GlucoPilot.Api/", "GlucoPilot.Api/"]
COPY ["GlucoPilot.Data.Migrators.MSSQL/", "GlucoPilot.Data.Migrators.MSSQL/"]
COPY ["GlucoPilot.Data/", "GlucoPilot.Data/"]
COPY ["GlucoPilot.Identity/", "GlucoPilot.Identity/"]
COPY ["GlucoPilot.AspNetCore", "GlucoPilot.AspNetCore"]
COPY ["GlucoPilot.Mail", "GlucoPilot.Mail"]
COPY ["GlucoPilot.LibreLinkClient", "GlucoPilot.LibreLinkClient"]
COPY ["GlucoPilot.Sync.LibreLink", "GlucoPilot.Sync.LibreLink"]
COPY ["GlucoPilot.Resources", "GlucoPilot.Resources"]
WORKDIR "/src/GlucoPilot.Api"
RUN dotnet build "GlucoPilot.Api.csproj" -c "$BuildConfiguration" -o /app/build

FROM build AS publish
ARG BuildConfiguration=Release
RUN dotnet publish "GlucoPilot.Api.csproj" -c "$BuildConfiguration" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER appuser

ENTRYPOINT ["dotnet", "GlucoPilot.Api.dll"]
