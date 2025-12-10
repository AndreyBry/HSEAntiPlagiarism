FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY HSEAntiPlagiarism.sln ./
COPY src/ ./src/
RUN dotnet restore
RUN dotnet publish ./src/FileAnalysisService/FileAnalysisService.Api/FileAnalysisService.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FileAnalysisService.Api.dll"]
