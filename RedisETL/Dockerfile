# Use the official .NET SDK image as a build environment
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY /RedisETL/*.csproj ./
RUN dotnet restore

# Copy the remaining source code
COPY . ./

# Build the application
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
WORKDIR /app
COPY --from=build-env /app/out ./

# Set environment variables if needed
# ENV VariableName=Value

# Command to run the application
ENTRYPOINT ["dotnet", "RedisETL.dll"]