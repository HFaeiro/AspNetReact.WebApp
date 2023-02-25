FROM mcr.microsoft.com/dotnet/sdk:6.0
ADD ./ /app
ENV ASPNETCORE_URLS=http://*:${PORT}
WORKDIR /app
ENTRYPOINT [ "dotnet", "ASP.Back.dll" ]