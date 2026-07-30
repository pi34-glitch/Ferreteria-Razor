# Etapa de compilación del cliente React (isla de "Nueva venta")
FROM node:20-alpine AS clientbuild

WORKDIR /client

COPY clientapp/nueva-venta/package*.json ./

RUN npm ci

COPY clientapp/nueva-venta/. .

RUN npm run build


# Etapa de compilación del cliente React de tablas (Productos,
# Inventario, Historial de ventas)
FROM node:20-alpine AS tablasbuild

WORKDIR /client

COPY clientapp/tablas/package*.json ./

RUN npm ci

COPY clientapp/tablas/. .

RUN npm run build


# Etapa de compilación de .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["Ferreteria.csproj", "./"]

RUN dotnet restore "Ferreteria.csproj"

COPY . .

COPY --from=clientbuild /client/dist/. ./wwwroot/js/nueva-venta/
COPY --from=tablasbuild /client/dist/. ./wwwroot/js/tablas/

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

# Render (y otros contenedores livianos) tienen un límite bajo de
# inotify watchers; sin esto, el host intenta vigilar cambios en
# appsettings.json con un FileSystemWatcher y la app crashea al
# arrancar con "configured user limit on inotify instances reached".
ENV DOTNET_hostBuilder__reloadConfigOnChange=false

EXPOSE 8080

COPY --from=build /app/publish .

# Render inyecta la variable PORT en tiempo de ejecución; el
# entrypoint arma la URL de escucha dinámicamente en vez de
# depender de un puerto fijo en el build.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT} dotnet Ferreteria.dll"]