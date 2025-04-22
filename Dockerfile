# Stage 1: Build the application
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
ARG ENVIRONMENT=Development

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}

# Copy everything first to ensure full context (especially for analyzers and props)
COPY . .

# Restore dependencies — make sure it has access to all required files
RUN dotnet restore ./HealthDevice/HealthDevice.csproj -r linux-x64

# Build for good measure (optional, but best practice)
RUN dotnet build ./HealthDevice/HealthDevice.csproj -c Release -r linux-x64 --no-restore

# Publish
RUN dotnet publish ./HealthDevice/HealthDevice.csproj -c Release -r linux-x64 -o /app/publish --self-contained false --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5171

ENTRYPOINT ["dotnet", "HealthDevice.dll"]