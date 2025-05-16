# S? d?ng .NET 9.0 SDK image ?? build ?ng d?ng
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj và restore dependencies
COPY ["BatteryShop.Gateway.csproj", "./"]
RUN dotnet restore "BatteryShop.Gateway.csproj"

# Copy t?t c? files và build
COPY . .
RUN dotnet build "BatteryShop.Gateway.csproj" -c Release -o /app/build

# Publish ?ng d?ng
FROM build AS publish
RUN dotnet publish "BatteryShop.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# C?u hình bi?n môi tr??ng
ENV ASPNETCORE_URLS=http://+:8080;https://+:8081
ENV ASPNETCORE_ENVIRONMENT=Production

# M? ports
EXPOSE 8080
EXPOSE 8081

# Kh?i ??ng ?ng d?ng
ENTRYPOINT ["dotnet", "BatteryShop.Gateway.dll"]