FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/services/AdminService/AdminService.csproj", "src/services/AdminService/"]
COPY ["src/building-block/Shared.Events/Shared.Events.csproj", "src/building-block/Shared.Events/"]
RUN dotnet restore "src/services/AdminService/AdminService.csproj"

COPY . .
WORKDIR /src/src/services/AdminService
RUN dotnet publish "AdminService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AdminService.dll"]
