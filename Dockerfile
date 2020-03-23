#-------------------------------------------- BUILD
FROM microsoft/dotnet:2.1-sdk-alpine AS build_publish
WORKDIR /src

COPY ["JWTProjectCore.Api", "JWTProjectCore.Api"]
COPY ["JWTProjectCore.Core", "JWTProjectCore.Core"]
COPY ["JWTProjectCore.sln", "JWTProjectCore.sln"]

RUN dotnet restore
RUN dotnet build -c Release -o /api/build
RUN dotnet publish -c Release -o /app/publish
#-------------------------------------------- RUN
FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine
WORKDIR /app
EXPOSE 80
COPY --from=build_publish /app/publish .
ENTRYPOINT ["dotnet", "JWTProjectCore.Api.dll"]
