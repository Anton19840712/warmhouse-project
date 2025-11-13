FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8083

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["DeviceService/DeviceService.csproj", "DeviceService/"]
RUN dotnet restore "DeviceService/DeviceService.csproj"
COPY . .
WORKDIR "/src/DeviceService"
RUN dotnet build "DeviceService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DeviceService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8083
ENTRYPOINT ["dotnet", "DeviceService.dll"]
