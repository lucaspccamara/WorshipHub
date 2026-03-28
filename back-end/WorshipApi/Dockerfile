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
    /p:UseAppHost=false

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

# Instala tzdata para suporte a fusos horários IANA (ex: America/Sao_Paulo)
RUN apk add --no-cache tzdata

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WorshipApi.dll"]