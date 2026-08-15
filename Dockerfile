FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish WebApi/WebApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/cert.pfx /app/cert.pfx
RUN mkdir -p /app/uploads
EXPOSE 5000 5002
ENTRYPOINT ["dotnet", "WebApi.dll"]
