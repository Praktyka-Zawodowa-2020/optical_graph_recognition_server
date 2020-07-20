FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get update
RUN apt-get -y upgrade 
RUN apt-get -y install python3
RUN apt-get -y install python3-pip
RUN apt-get install -y libsm6 libxext6 libxrender-dev
RUN yes | pip3 install opencv-python

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Api/Api.csproj", "Api/"]
RUN dotnet restore "Api/Api.csproj"
COPY . .
WORKDIR "/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src/Api/Certs Certs/ 
ADD Api/Script Script/
ENTRYPOINT ["dotnet", "Api.dll"]