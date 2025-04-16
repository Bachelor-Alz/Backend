# Stage 1: Build the application
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH

WORKDIR /app

ARG ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}

COPY Backend.sln ./ 
COPY HealthDevice/*.csproj ./HealthDevice/
COPY HealthDevice/Migrations/*.cs ./HealthDevice/Migrations/

# Restore dependencies
RUN dotnet restore Backend.sln -a $TARGETARCH

COPY . .

RUN dotnet publish -a $TARGETARCH ./HealthDevice/HealthDevice.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

EXPOSE 5171

# Start the application
ENTRYPOINT ["dotnet", "HealthDevice.dll"]
