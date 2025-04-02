FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["GlucoPilot.Api/GlucoPilot.Api.csproj", "GlucoPilot.Api/"]
RUN dotnet restore "GlucoPilot.Api/GlucoPilot.Api.csproj"
COPY . .
WORKDIR "/src/GlucoPilot.Api"
RUN dotnet build "GlucoPilot.Api.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration=Release
RUN dotnet publish "GlucoPilot.Api.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GlucoPilot.Api.dll"]