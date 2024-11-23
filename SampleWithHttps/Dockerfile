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
COPY ["SampleWithHttps/SampleWithHttps.csproj", "SampleWithHttps/"]
COPY . .
WORKDIR "/src/SampleWithHttps"
CMD dotnet run --no-launch-profile

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SampleWithHttps/SampleWithHttps.csproj", "SampleWithHttps/"]
RUN dotnet restore "./SampleWithHttps/SampleWithHttps.csproj"
COPY . .
WORKDIR "/src/SampleWithHttps"
RUN dotnet build "./SampleWithHttps.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SampleWithHttps.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleWithHttps.dll"]