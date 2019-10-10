FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app

COPY ./Proxymit.Core/Proxymit.Core.csproj ./Proxymit.Core/Proxymit.Core.csproj
COPY ./Proxymit.Host/Proxymit.Host.csproj ./Proxymit.Host/Proxymit.Host.csproj
COPY *.sln ./
RUN dotnet restore

COPY . ./

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "Proxymit.Host.dll"]
