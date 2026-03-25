FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY datalabel-api.sln ./
COPY src/DataLabeling.API/DataLabeling.API.csproj src/DataLabeling.API/
COPY src/DataLabeling.BLL/DataLabeling.BLL.csproj src/DataLabeling.BLL/
COPY src/DataLabeling.DAL/DataLabeling.DAL.csproj src/DataLabeling.DAL/
COPY src/DataLabeling.Entities/DataLabeling.Entities.csproj src/DataLabeling.Entities/

RUN dotnet restore

COPY . .

WORKDIR /src/src/DataLabeling.API
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

RUN adduser --disabled-password --gecos '' appuser && \
    chown -R appuser /app

USER appuser

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DataLabeling.API.dll"]
