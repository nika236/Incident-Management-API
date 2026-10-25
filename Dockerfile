FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY IncidentManagement.sln ./
COPY src/IncidentManagement.Api/IncidentManagement.Api.csproj src/IncidentManagement.Api/
COPY src/IncidentManagement.Application/IncidentManagement.Application.csproj src/IncidentManagement.Application/
COPY src/IncidentManagement.Domain/IncidentManagement.Domain.csproj src/IncidentManagement.Domain/
COPY src/IncidentManagement.Infrastructure/IncidentManagement.Infrastructure.csproj src/IncidentManagement.Infrastructure/

RUN dotnet restore src/IncidentManagement.Api/IncidentManagement.Api.csproj

COPY src/ src/
RUN dotnet publish src/IncidentManagement.Api/IncidentManagement.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IncidentManagement.Api.dll"]
