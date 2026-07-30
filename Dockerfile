# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["Ferreteria.csproj", "./"]

RUN dotnet restore "Ferreteria.csproj"

COPY . .

RUN dotnet publish "Ferreteria.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080

EXPOSE 8080

COPY --from=build /app/publish .

# Render inyecta la variable PORT en tiempo de ejecución; el
# entrypoint arma la URL de escucha dinámicamente en vez de
# depender de un puerto fijo en el build.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT} dotnet Ferreteria.dll"]