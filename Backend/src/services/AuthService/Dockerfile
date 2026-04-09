FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/services/AuthService/AuthService.csproj", "src/services/AuthService/"]
COPY ["src/building-block/Shared.Events/Shared.Events.csproj", "src/building-block/Shared.Events/"]
RUN dotnet restore "src/services/AuthService/AuthService.csproj"

COPY . .
WORKDIR /src/src/services/AuthService
RUN dotnet publish "AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthService.dll"]
