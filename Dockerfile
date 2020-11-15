# restore packages
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY *.sln .
COPY Client/*.csproj Client/
COPY Server/*.csproj Server/
COPY Shared/*.csproj Shared/
# COPY Tests/*.csproj Tests/
RUN dotnet restore
COPY . .

# todo: run tests
# FROM build AS testing
# WORKDIR /src/Server
# RUN dotnet build
# WORKDIR /src/Tests
# RUN dotnet test

# publish
FROM build AS publish
WORKDIR /src/Server
RUN dotnet publish -c Release -o /src/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=publish /src/publish .

# Optional: Set these here if not in docker-compose.yml or appsettings.json
# ENV DatabaseOptions__ConnectionString=$CONNECTION_STRING
# ENV ImageUploadApiOptions__ApiKey=$IMAGE_UPLOAD_APIKEY
# ENV JwtOptions__SecurityKey=$JWT_SECURITYKEY
# ENV VapidOptions__PrivateKey=$VAPID_PRIVATEKEY

# if using heroku, comment this and uncomment the CMD line below
ENTRYPOINT ["dotnet", "Localist.Server.dll"] 
# CMD ASPNETCORE_URLS=http://*:$PORT dotnet Localist.Server.dll