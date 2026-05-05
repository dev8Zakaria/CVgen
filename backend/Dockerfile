FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AiCv.Backend.sln ./
COPY src/AiCv.Api/AiCv.Api.csproj src/AiCv.Api/
RUN dotnet restore AiCv.Backend.sln

COPY src/ src/
RUN dotnet publish src/AiCv.Api/AiCv.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://0.0.0.0:5000
EXPOSE 5000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AiCv.Api.dll"]
