FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/AiCv.BffGateway/AiCv.BffGateway.csproj src/AiCv.BffGateway/
RUN dotnet restore src/AiCv.BffGateway/AiCv.BffGateway.csproj

COPY src/AiCv.BffGateway/ src/AiCv.BffGateway/
RUN dotnet publish src/AiCv.BffGateway/AiCv.BffGateway.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AiCv.BffGateway.dll"]
