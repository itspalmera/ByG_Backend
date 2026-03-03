# Usa la imagen oficial de .NET 9 SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia el archivo de proyecto y restaura dependencias
COPY ["ByG_Backend.csproj", "./"]
RUN dotnet restore "ByG_Backend.csproj"

# Copia el resto del código y compila
COPY . .
RUN dotnet publish "ByG_Backend.csproj" -c Release -o /app/publish

# Usa la imagen ligera de ASP.NET 9 para ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Configura el puerto para Render (Render usa 8080 por defecto internamente)
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ByG_Backend.dll"]