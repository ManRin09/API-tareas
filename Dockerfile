FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solución y archivos de proyecto para aprovechar la caché de restore
COPY ProjectManagerApi.sln ./
COPY src/ProjectManagerApi.Domain/ProjectManagerApi.Domain.csproj src/ProjectManagerApi.Domain/
COPY src/ProjectManagerApi.Application/ProjectManagerApi.Application.csproj src/ProjectManagerApi.Application/
COPY src/ProjectManagerApi.Infrastructure/ProjectManagerApi.Infrastructure.csproj src/ProjectManagerApi.Infrastructure/
COPY src/ProjectManagerApi.API/ProjectManagerApi.API.csproj src/ProjectManagerApi.API/

RUN dotnet restore src/ProjectManagerApi.API/ProjectManagerApi.API.csproj

# Copiar código fuente y publicar la API
COPY src/ src/
RUN dotnet publish src/ProjectManagerApi.API/ProjectManagerApi.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
USER $APP_UID

ENTRYPOINT ["dotnet", "ProjectManagerApi.API.dll"]
