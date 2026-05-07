FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./backend/OnlyOfficeCollab.sln ./
COPY ./backend/src/Api/Api.csproj ./src/Api/
COPY ./backend/src/Application/Application.csproj ./src/Application/
COPY ./backend/src/Domain/Domain.csproj ./src/Domain/
COPY ./backend/src/Infrastructure/Infrastructure.csproj ./src/Infrastructure/
RUN dotnet restore ./src/Api/Api.csproj

COPY ./backend/src ./src
RUN dotnet publish ./src/Api/Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000
COPY --from=build /app/publish .
VOLUME ["/app/data"]
EXPOSE 5000
ENTRYPOINT ["dotnet", "Api.dll"]
