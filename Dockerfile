# ============================================
# STAGE 1: Build Stage
# ============================================
# Using SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory inside container
WORKDIR /src

# Copy project file first (for better caching)
# Docker caches layers - if csproj hasn't changed, 
# dependencies won't be restored again
COPY ["BankingAPI.csproj", "./"]

# Restore NuGet packages
RUN dotnet restore "BankingAPI.csproj"

# Copy the rest of the source code
COPY . .

# Build the application in Release mode
RUN dotnet build "BankingAPI.csproj" -c Release -o /app/build

# ============================================
# STAGE 2: Publish Stage
# ============================================
FROM build AS publish

# Publish the application (creates optimized output)
RUN dotnet publish "BankingAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# STAGE 3: Runtime Stage (Final Image)
# ============================================
# Using smaller runtime image (no SDK needed)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Set working directory
WORKDIR /app

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser

# Copy published files from publish stage
COPY --from=publish /app/publish .

# Copy data files
COPY Data/ ./Data/

# Set ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port 8080 (non-privileged port)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/api/health/live || exit 1

# Run the application
ENTRYPOINT ["dotnet", "BankingAPI.dll"]