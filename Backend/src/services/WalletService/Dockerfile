FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/services/WalletService/WalletService.csproj", "src/services/WalletService/"]
COPY ["src/building-block/Shared.Events/Shared.Events.csproj", "src/building-block/Shared.Events/"]
RUN dotnet restore "src/services/WalletService/WalletService.csproj"

COPY . .
WORKDIR /src/src/services/WalletService
RUN dotnet publish "WalletService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WalletService.dll"]
