# Use Microsoft's official .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY DanialNetFood.Web/*.csproj ./DanialNetFood.Web/
RUN dotnet restore DanialNetFood.Web/DanialNetFood.Web.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish DanialNetFood.Web/DanialNetFood.Web.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .

# Entry point
ENTRYPOINT ["dotnet", "DanialNetFood.Web.dll"]
