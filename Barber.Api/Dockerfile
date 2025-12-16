FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Barber.Api/Barber.Api.csproj", "Barber.Api/"]
COPY ["Barber.Application/Barber.Application.csproj", "Barber.Application/"]
COPY ["Barber.Domain/Barber.Domain.csproj", "Barber.Domain/"]
COPY ["Barber.Infrastructure/Barber.Infrastructure.csproj", "Barber.Infrastructure/"]
RUN dotnet restore "Barber.Api/Barber.Api.csproj"
COPY . .
WORKDIR "/src/Barber.Api"
RUN dotnet build "./Barber.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Barber.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Barber.Api.dll"]
