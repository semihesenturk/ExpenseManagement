FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Services/Notification/ExpenseManagement.NotificationAPI/ExpenseManagement.NotificationAPI.csproj", "src/Services/Notification/ExpenseManagement.NotificationAPI/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
COPY ["src/Shared/Shared.Kernel/Shared.Kernel.csproj", "src/Shared/Shared.Kernel/"]

RUN dotnet restore "src/Services/Notification/ExpenseManagement.NotificationAPI/ExpenseManagement.NotificationAPI.csproj"

COPY . .
WORKDIR "/src/src/Services/Notification/ExpenseManagement.NotificationAPI"
RUN dotnet build "ExpenseManagement.NotificationAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ExpenseManagement.NotificationAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExpenseManagement.NotificationAPI.dll"]