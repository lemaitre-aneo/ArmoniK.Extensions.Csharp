#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Common/StreamWrapper/tests/Server/ArmoniK.Extensions.Common.StreamWrapper.Tests.Server.csproj", "Common/StreamWrapper/tests/Server/"]
COPY ["Common/StreamWrapper/tests/Common/ArmoniK.Extensions.Common.StreamWrapper.Tests.Common.csproj", "Common/StreamWrapper/tests/Common/"]
COPY ["Common/StreamWrapper/src/ArmoniK.Extensions.Common.StreamWrapper.csproj", "Common/StreamWrapper/src/"]
RUN dotnet restore "Common/StreamWrapper/tests/Server/ArmoniK.Extensions.Common.StreamWrapper.Tests.Server.csproj"
COPY . .
WORKDIR "/src/Common/StreamWrapper/tests/Server"
RUN dotnet build "ArmoniK.Extensions.Common.StreamWrapper.Tests.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.Extensions.Common.StreamWrapper.Tests.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmoniK.Extensions.Common.StreamWrapper.Tests.Server.dll"]