FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source
COPY src/ src/
COPY Directory.Build.props .
COPY global.json .
COPY vera.sln .
RUN dotnet publish -c Release -o /app --no-self-contained --nologo -v q src/Vera.Host/Vera.Host.csproj

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Vera.Host.dll"]