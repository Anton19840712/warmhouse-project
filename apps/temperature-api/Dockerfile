FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TemperatureApi/TemperatureApi.csproj", "TemperatureApi/"]
RUN dotnet restore "TemperatureApi/TemperatureApi.csproj"
COPY . .
WORKDIR "/src/TemperatureApi"
RUN dotnet build "TemperatureApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TemperatureApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8081
ENTRYPOINT ["dotnet", "TemperatureApi.dll"]
