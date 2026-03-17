FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore src/Services/Fee/BankMore.Fee.Worker/BankMore.Fee.Worker.csproj
RUN dotnet publish src/Services/Fee/BankMore.Fee.Worker/BankMore.Fee.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BankMore.Fee.Worker.dll"]
