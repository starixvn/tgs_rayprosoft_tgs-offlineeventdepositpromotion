#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.OfflineEventDepositPromotion/Tgs.OfflineEventDepositPromotion.csproj", "Tgs.OfflineEventDepositPromotion/"]
COPY ["OfflineEventDepositPromotion.Core/OfflineEventDepositPromotion.Core.csproj", "OfflineEventDepositPromotion.Core/"]
COPY ["OfflineEventDepositPromotion.Repositories/OfflineEventDepositPromotion.Repositories.csproj", "OfflineEventDepositPromotion.Repositories/"]
RUN dotnet restore "Tgs.OfflineEventDepositPromotion/Tgs.OfflineEventDepositPromotion.csproj"
COPY . .
WORKDIR "/src/Tgs.OfflineEventDepositPromotion"
RUN dotnet build "Tgs.OfflineEventDepositPromotion.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.OfflineEventDepositPromotion.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.OfflineEventDepositPromotion.dll"]