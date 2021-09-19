FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine-amd64 AS build

COPY ./Client /src/Client
COPY ./Server /src/Server
COPY ./Shared /src/Shared

WORKDIR /src

RUN dotnet publish ./Server -o /app -c Release -r alpine-x64

FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine-amd64

COPY --from=build /app /app

WORKDIR /app

EXPOSE 80

CMD ./BlazorChat.Server