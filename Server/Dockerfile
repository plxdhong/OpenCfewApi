#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal-arm64v8 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:21001
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal-arm64v8 AS build
WORKDIR /src
ENV ASPNETCORE_URLS=http://+:21001
COPY ["Server/CFEW.Server.csproj", "Server/"]
COPY ["XraySDK/XraySDK.csproj", "XraySDK/"]
COPY ["Shared/CFEW.Shared.csproj", "Shared/"]
RUN dotnet restore "Server/CFEW.Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "CFEW.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CFEW.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CFEW.Server.dll"]
