# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ExpenseTracker/ExpenseTracker.API.csproj ExpenseTracker/
COPY ExpenseTracker.Application/ExpenseTracker.Application.csproj ExpenseTracker.Application/
COPY ExpenseTracker.Domain/ExpenseTracker.Domain.csproj ExpenseTracker.Domain/
COPY ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj ExpenseTracker.Infrastructure/
COPY ExpenseTracker.Persistence/ExpenseTracker.Persistence.csproj ExpenseTracker.Persistence/

RUN dotnet restore ExpenseTracker/ExpenseTracker.API.csproj

# Copy all source code and publish
COPY . .
RUN dotnet publish ExpenseTracker/ExpenseTracker.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

COPY --from=build /app/publish .

RUN chown -R appuser:appgroup /app
USER appuser

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "ExpenseTracker.API.dll"]
