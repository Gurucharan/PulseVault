# Step 1: Use the .NET 10 SDK image to build and publish the API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["PulseVault.Api/PulseVault.Api.csproj", "PulseVault.Api/"]
RUN dotnet restore "PulseVault.Api/PulseVault.Api.csproj"

# Copy all source code and publish release binaries
COPY . .
WORKDIR "/src/PulseVault.Api"
RUN dotnet build "PulseVault.Api.csproj" -c Release -o /app/build
RUN dotnet publish "PulseVault.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Step 2: Use lightweight ASP.NET 10 runtime to run the published DLL
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Expose port 8080 (Render's default port)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copy output binaries and start the API
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PulseVault.Api.dll"]