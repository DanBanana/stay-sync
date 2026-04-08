FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY StaySync.sln .
COPY src/StaySync.Domain/StaySync.Domain.csproj src/StaySync.Domain/
COPY src/StaySync.Application/StaySync.Application.csproj src/StaySync.Application/
COPY src/StaySync.Infrastructure/StaySync.Infrastructure.csproj src/StaySync.Infrastructure/
COPY src/StaySync.API/StaySync.API.csproj src/StaySync.API/
COPY tests/StaySync.Application.Tests/StaySync.Application.Tests.csproj tests/StaySync.Application.Tests/

RUN dotnet restore

COPY . .
RUN dotnet publish src/StaySync.API/StaySync.API.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "StaySync.API.dll"]
