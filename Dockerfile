# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["Lunatune.csproj", "."]
RUN dotnet restore "Lunatune.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "Lunatune.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Lunatune.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "Lunatune.dll"]
