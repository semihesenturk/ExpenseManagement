FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Services/Expense/Expense.API/Expense.API.csproj", "src/Services/Expense/Expense.API/"]
COPY ["src/Services/Expense/Expense.Application/Expense.Application.csproj", "src/Services/Expense/Expense.Application/"]
COPY ["src/Services/Expense/Expense.Domain/Expense.Domain.csproj", "src/Services/Expense/Expense.Domain/"]
COPY ["src/Services/Expense/Expense.Infrastructure/Expense.Infrastructure.csproj", "src/Services/Expense/Expense.Infrastructure/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
COPY ["src/Shared/Shared.Kernel/Shared.Kernel.csproj", "src/Shared/Shared.Kernel/"]

RUN dotnet restore "src/Services/Expense/Expense.API/Expense.API.csproj"

COPY . .
WORKDIR "/src/src/Services/Expense/Expense.API"
RUN dotnet build "Expense.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Expense.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Expense.API.dll"]