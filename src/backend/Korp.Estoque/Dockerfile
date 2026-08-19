FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# context = src/backend/
COPY Korp.SharedKernel/ Korp.SharedKernel/
COPY Korp.Estoque/ Korp.Estoque/

RUN dotnet restore Korp.Estoque/Korp.Estoque.API/Korp.Estoque.API.csproj

RUN dotnet publish Korp.Estoque/Korp.Estoque.API/Korp.Estoque.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "Korp.Estoque.API.dll"]
