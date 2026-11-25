FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SportsCenter.Api/SportsCenter.Api.csproj", "SportsCenter.Api/"]
RUN dotnet restore "SportsCenter.Api/SportsCenter.Api.csproj"

COPY SportsCenter.Api/ SportsCenter.Api/
WORKDIR /src/SportsCenter.Api
RUN dotnet publish "SportsCenter.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SportsCenter.Api.dll"]
