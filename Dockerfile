# Stage 1: Build the application
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH

WORKDIR /app

# Set the default environment to Development
ARG ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}

# Copy the solution and project files
COPY Backend.sln ./ 
COPY HealthDevice/*.csproj ./HealthDevice/
COPY HealthDevice/Migrations/*.cs ./HealthDevice/Migrations/

# Restore dependencies for all projects in the solution
RUN dotnet restore Backend.sln

# Copy the entire source code for the projects
COPY . .

# Build the main application with the architecture specified
RUN dotnet build ./HealthDevice/HealthDevice.csproj -c Release -o /app/build -a $TARGETARCH

# Publish the main application with the architecture specified
RUN dotnet publish ./HealthDevice/HealthDevice.csproj -c Release -o /app/publish -a $TARGETARCH --no-restore

# Expose the port for the app
EXPOSE 5171

# Set the entry point for development or production
ENTRYPOINT ["sh", "-c", "if [ \"$ASPNETCORE_ENVIRONMENT\" = 'Development' ]; then dotnet watch run --project HealthDevice/HealthDevice.csproj --urls http://+:5171; else dotnet /app/publish/HealthDevice.dll; fi"]
