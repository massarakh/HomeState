#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TG_Bot/TG_Bot.csproj", "TG_Bot/"]
RUN dotnet restore "TG_Bot/TG_Bot.csproj"
COPY . .
WORKDIR "/src/TG_Bot"
RUN dotnet build "TG_Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TG_Bot.csproj" -c Release -o /app/publish

FROM katalonstudio/katalon
RUN apt-get -y update
RUN apt-get install -y ffmpeg

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TG_Bot.dll"]