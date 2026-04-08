FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AccountService.slnx ./
COPY AccountService/AccountService.csproj AccountService/
COPY AccountService.Tests/AccountService.Tests.csproj AccountService.Tests/

RUN dotnet restore

COPY . .
RUN dotnet publish AccountService/AccountService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AccountService.dll"]
