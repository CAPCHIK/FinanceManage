FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY FinanceManage/Server/*.csproj ./FinanceManage/Server/
COPY FinanceManage/Client/*.csproj ./FinanceManage/Client/
COPY FinanceManage/Shared/*.csproj ./FinanceManage/Shared/
RUN dotnet restore

# copy everything else and build app
COPY FinanceManage/. ./FinanceManage/
WORKDIR /source/FinanceManage/Server/
RUN dotnet publish -c release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "FinanceManage.Server.dll"]