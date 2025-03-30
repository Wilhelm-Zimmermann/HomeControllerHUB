FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 6001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj", "HomeControllerHUB.Api/"]
COPY ["src/HomeControllerHUB.Infra/HomeControllerHUB.Infra.csproj", "HomeControllerHUB.Infra/"]
COPY ["src/HomeControllerHUB.Domain/HomeControllerHUB.Domain.csproj", "HomeControllerHUB.Domain/"]
COPY ["src/HomeControllerHUB.Application/HomeControllerHUB.Application.csproj", "HomeControllerHUB.Application/"]
COPY ["src/HomeControllerHUB.Shared/HomeControllerHUB.Shared.csproj", "HomeControllerHUB.Shared/"]
COPY ["src/HomeControllerHUB.Globalization/HomeControllerHUB.Globalization.csproj", "HomeControllerHUB.Globalization/"]

RUN dotnet restore "HomeControllerHUB.Api/HomeControllerHUB.Api.csproj"

COPY src/ .

WORKDIR "/src/HomeControllerHUB.Api"
RUN dotnet build "HomeControllerHUB.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HomeControllerHUB.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HomeControllerHUB.Api.dll"]