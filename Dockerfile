# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY src/SmartPlanner.csproj src/
RUN dotnet restore "src/SmartPlanner.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src"
RUN dotnet build "SmartPlanner.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SmartPlanner.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create directory for data protection keys
RUN mkdir -p /var/lib/smartplanner-keys && chmod 755 /var/lib/smartplanner-keys

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Expose port
EXPOSE 8080

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app && chown -R appuser /var/lib/smartplanner-keys
USER appuser

ENTRYPOINT ["dotnet", "SmartPlanner.dll"]
