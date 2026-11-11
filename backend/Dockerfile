# Multi-stage Dockerfile for .NET 8 Web API
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files for layer caching
COPY SchoolAdmission.sln ./
COPY src/SchoolAdmission.Api/SchoolAdmission.Api.csproj ./src/SchoolAdmission.Api/
COPY tests/SchoolAdmission.Tests/SchoolAdmission.Tests.csproj ./tests/SchoolAdmission.Tests/

RUN dotnet restore ./src/SchoolAdmission.Api/SchoolAdmission.Api.csproj

# Copy full source and publish
COPY src/ ./src/
WORKDIR /app/src/SchoolAdmission.Api
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV HOLD_DURATION_SECONDS=600

EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "SchoolAdmission.Api.dll"]

