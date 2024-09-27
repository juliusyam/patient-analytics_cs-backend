FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /src
COPY ["PatientAnalytics/PatientAnalytics.csproj", "PatientAnalytics/"]

RUN dotnet restore "PatientAnalytics/PatientAnalytics.csproj"

COPY . .
WORKDIR "/src/PatientAnalytics"

RUN dotnet publish "PatientAnalytics.csproj" -c Release -o /app

WORKDIR /app
EXPOSE 8080
ENTRYPOINT ["dotnet", "PatientAnalytics.dll"]