# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["everdadeisso.csproj", "."]
RUN dotnet restore "./everdadeisso.csproj"
COPY . .
RUN dotnet publish "./everdadeisso.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render usa porta 10000+ por padrão, mas expor 80 funciona bem
EXPOSE 80
ENTRYPOINT ["dotnet", "everdadeisso.dll"]