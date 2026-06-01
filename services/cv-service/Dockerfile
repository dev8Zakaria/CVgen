FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/AiCv.CvService/AiCv.CvService.csproj src/AiCv.CvService/
RUN dotnet restore src/AiCv.CvService/AiCv.CvService.csproj

COPY src/AiCv.CvService/ src/AiCv.CvService/
RUN dotnet publish src/AiCv.CvService/AiCv.CvService.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AiCv.CvService.dll"]
