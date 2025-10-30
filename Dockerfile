# Multi-stage build for optimized production image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["TafsilkPlatform.Web/TafsilkPlatform.Web.csproj", "TafsilkPlatform.Web/"]
RUN dotnet restore "TafsilkPlatform.Web/TafsilkPlatform.Web.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/TafsilkPlatform.Web"
RUN dotnet build "TafsilkPlatform.Web.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "TafsilkPlatform.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage - runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install SQL Server tools (optional, for migrations)
# RUN apt-get update && apt-get install -y curl gnupg \
#&& curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
#  && curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list > /etc/apt/sources.list.d/mssql-release.list \
#     && apt-get update && ACCEPT_EULA=Y apt-get install -y mssql-tools18 unixodbc-dev \
#     && apt-get clean && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN useradd -m -u 1000 appuser && chown -R appuser /app
USER appuser

# Copy published files
COPY --from=publish --chown=appuser:appuser /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start application
ENTRYPOINT ["dotnet", "TafsilkPlatform.Web.dll"]
