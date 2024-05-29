FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PlusNine.Api/PlusNine.Api.csproj", "PlusNine.Api/"]
COPY ["PlusNine.DataService/PlusNine.DataService.csproj", "PlusNine.DataService/"]
COPY ["PlusNine.Entities/PlusNine.Entities.csproj", "PlusNine.Entities/"]
RUN dotnet restore "PlusNine.Api/PlusNine.Api.csproj"
COPY . .
WORKDIR "/src/PlusNine.Api"
RUN dotnet build "PlusNine.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build as publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PlusNine.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "PlusNine.Api.dll"]
