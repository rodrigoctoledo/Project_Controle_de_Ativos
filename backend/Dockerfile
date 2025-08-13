# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia CADA csproj (paths do seu repo)
COPY AssetControl.Api/AssetControl.Api.csproj AssetControl.Api/
COPY AssetControl.Application/AssetControl.Application.csproj AssetControl.Application/
COPY AssetControl.Domain/AssetControl.Domain.csproj AssetControl.Domain/

# Restaura o projeto web
RUN dotnet restore AssetControl.Api/AssetControl.Api.csproj

# Copia o restante do código
COPY . .

# Publica explicitamente o csproj (evita publicar a .sln por engano)
RUN dotnet publish /src/AssetControl.Api/AssetControl.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ===== runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:5000
EXPOSE 5000

# Pasta de dados (SQLite) no container
RUN mkdir -p /app/data

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AssetControl.Api.dll"]
