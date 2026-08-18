FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY KnowHowApi/KnowHowApi.csproj KnowHowApi/
RUN dotnet restore KnowHowApi/KnowHowApi.csproj

COPY KnowHowApi/ KnowHowApi/
RUN dotnet publish KnowHowApi/KnowHowApi.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "KnowHowApi.dll"]
