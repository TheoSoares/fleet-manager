FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CarSalesApi.csproj", "."]
RUN dotnet restore "./CarSalesApi.csproj"
COPY . .
RUN dotnet build "CarSalesApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CarSalesApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CarSalesApi.dll"]
