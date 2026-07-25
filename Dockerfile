FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore SistemaGestion.slnx
RUN dotnet publish src/SistemaGestion.Web/SistemaGestion.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --retries=3 CMD curl --fail http://localhost:8080/health/live || exit 1
ENTRYPOINT ["dotnet", "SistemaGestion.Web.dll"]
