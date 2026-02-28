# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

WORKDIR /src

# Copia solution
COPY WorshipApi/WorshipApi.sln ./

# Copia csproj individualmente para melhor cache
COPY WorshipApi/WorshipApi.csproj WorshipApi/
COPY WorshipApplication/WorshipApplication.csproj WorshipApplication/
COPY WorshipDomain/WorshipDomain.csproj WorshipDomain/
COPY WorshipInfra/WorshipInfra.csproj WorshipInfra/

# Restore
RUN dotnet restore WorshipApi/WorshipApi.csproj

# Copia o restante do código
COPY . .

WORKDIR /src/WorshipApi

# Publish otimizado
RUN dotnet publish -c Release -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=true

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WorshipApi.dll"]