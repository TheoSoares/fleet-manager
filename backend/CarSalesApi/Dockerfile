FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CarSalesApi.csproj", "."]
RUN dotnet restore "./CarSalesApi.csproj"

COPY . .
RUN dotnet build "CarSalesApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CarSalesApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CarSalesApi.dll"]
