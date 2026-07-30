# Step 1: Use the official .NET 8 ASP.NET runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Step 2: Use the SDK image to build and publish the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["MiniWallet.Api/MiniWallet.Api.csproj", "MiniWallet.Api/"]
RUN dotnet restore "MiniWallet.Api/MiniWallet.Api.csproj"

# Copy all source files and build
COPY . .
WORKDIR "/src/MiniWallet.Api"
RUN dotnet build "MiniWallet.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Step 3: Publish the built app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MiniWallet.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Step 4: Final stage - run the published application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiniWallet.Api.dll"]