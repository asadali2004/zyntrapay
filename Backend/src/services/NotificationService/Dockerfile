FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/services/NotificationService/NotificationService.csproj", "src/services/NotificationService/"]
COPY ["src/building-block/Shared.Events/Shared.Events.csproj", "src/building-block/Shared.Events/"]
RUN dotnet restore "src/services/NotificationService/NotificationService.csproj"

COPY . .
WORKDIR /src/src/services/NotificationService
RUN dotnet publish "NotificationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NotificationService.dll"]
