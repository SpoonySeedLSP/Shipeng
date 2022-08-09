#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Shipeng.Hosting/Shipeng.Hosting.csproj", "src/Shipeng.Hosting/"]
COPY ["src/Shipeng.Application/Shipeng.Application.csproj", "src/Shipeng.Application/"]
COPY ["src/Shipeng.Application.Contracts/Shipeng.Application.Contracts.csproj", "src/Shipeng.Application.Contracts/"]
COPY ["src/Shipeng.Domain.Shared/Shipeng.Domain.Shared.csproj", "src/Shipeng.Domain.Shared/"]
COPY ["src/Shipeng.Domain/Shipeng.Domain.csproj", "src/Shipeng.Domain/"]
COPY ["src/Shipeng.EntityFrameworkCore/Shipeng.EntityFrameworkCore.csproj", "src/Shipeng.EntityFrameworkCore/"]
RUN dotnet restore "src/Shipeng.Hosting/Shipeng.Hosting.csproj"
COPY . .
WORKDIR "/src/src/Shipeng.Hosting"
RUN dotnet build "Shipeng.Hosting.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shipeng.Hosting.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Shipeng.Hosting.dll"]