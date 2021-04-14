FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY build ./
ENV ASPNETCORE_URLS=http://*:5000
ENTRYPOINT ["dotnet", "FinanceManage.Server.dll"]