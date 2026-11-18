# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BlocoNaRua.Core/BlocoNaRua.Core.csproj", "BlocoNaRua.Core/"]
COPY ["BlocoNaRua.Domain/BlocoNaRua.Domain.csproj", "BlocoNaRua.Domain/"]
COPY ["BlocoNaRua.Data/BlocoNaRua.Data.csproj", "BlocoNaRua.Data/"]
COPY ["BlocoNaRua.Services/BlocoNaRua.Services.csproj", "BlocoNaRua.Services/"]
COPY ["BlocoNaRua.Restful/BlocoNaRua.Restful.csproj", "BlocoNaRua.Restful/"]
RUN dotnet restore "BlocoNaRua.Restful/BlocoNaRua.Restful.csproj"
COPY . .
RUN dotnet publish "BlocoNaRua.Restful/BlocoNaRua.Restful.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BlocoNaRua.Restful.dll"]
