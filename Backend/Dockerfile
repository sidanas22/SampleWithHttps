# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM base AS testCerts
ADD Certs/test-ca.crt /usr/local/share/ca-certificates/test-ca.crt
RUN chmod 644 /usr/local/share/ca-certificates/test-ca.crt && update-ca-certificates

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
ADD Certs/test-ca.crt /usr/local/share/ca-certificates/test-ca.crt
RUN chmod 644 /usr/local/share/ca-certificates/test-ca.crt && update-ca-certificates
WORKDIR /src
COPY ["Backend/Backend.csproj", "Backend/"]
COPY ["Shared/CommonLib.csproj", "Shared/"]
COPY . .
WORKDIR /src/Backend
CMD dotnet run --no-launch-profile

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Backend/Backend.csproj", "Backend/"]
COPY ["Shared/CommonLib.csproj", "Shared/"]
RUN dotnet restore "./Backend/Backend.csproj"
COPY . .
# Copy the source files from Shared to Shared inside the container
COPY ["Shared/", "Shared/"]

WORKDIR "/src/Backend"
RUN dotnet build "./Backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Backend.dll"]