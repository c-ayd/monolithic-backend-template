# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/Template.Api/Template.Api.csproj ./src/Template.Api/
COPY src/Template.Application/Template.Application.csproj ./src/Template.Application/
COPY src/Template.Domain/Template.Domain.csproj ./src/Template.Domain/
COPY src/Template.Infrastructure/Template.Infrastructure.csproj ./src/Template.Infrastructure/
COPY src/Template.Persistence/Template.Persistence.csproj ./src/Template.Persistence/

RUN dotnet restore src/Template.Api/Template.Api.csproj

COPY . .

WORKDIR /app/src/Template.Api
RUN dotnet publish -c Release -o /app/release

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/release .