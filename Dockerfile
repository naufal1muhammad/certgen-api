# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and restore dependencies
COPY . ./
RUN dotnet restore

# Build and publish the app to the /out directory
RUN dotnet publish -c Release -o out

# Stage 2: Run the app using a lighter runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    libpng-dev \
    libharfbuzz0b \
    libicu-dev \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/out ./

# Set the port and expose it
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

# Start the application
ENTRYPOINT ["dotnet", "CertGenAPI.dll"]