FROM mcr.microsoft.com/dotnet/sdk:5.0.300 AS development
ENV \
    DOCKER_ENV=1 \
    CONFIGURATION=Release \
    DLL_PATH=/app/bin/Server/${CONFIGURATION}/net5.0/publish/Identity.dll

WORKDIR /app

COPY . /app/
COPY certs/ /certs/
RUN if [ ! -f ${DLL_PATH} ]; then dotnet publish; fi

EXPOSE 80
ENTRYPOINT ["dotnet", "watch", "--project", "/app/src/Server", "run"]

FROM mcr.microsoft.com/dotnet/aspnet:5.0.6 AS production

WORKDIR /app
COPY --from=development /app/bin/Server/Release/net5.0/publish /app
COPY --from=development /certs /certs
COPY entrypoint.sh /

RUN  \
    apt-get update && \
    apt-get install -y python3 python3-pip && \
    pip3 install awscli && \
    chmod +x /entrypoint.sh

EXPOSE 80
ENTRYPOINT /entrypoint.sh

