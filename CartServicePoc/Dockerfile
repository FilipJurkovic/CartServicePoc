FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CartServicePoc/CartServicePoc.csproj", "CartServicePoc/"]
RUN dotnet restore "CartServicePoc/CartServicePoc.csproj"
COPY . .
WORKDIR "/src/CartServicePoc"
RUN dotnet build "CartServicePoc.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CartServicePoc.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CartServicePoc.dll"]
