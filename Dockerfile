FROM mcr.microsoft.com/dotnet/sdk:5.0.100-rc.2 AS development
ENV DOCKER_ENV 1
WORKDIR /app

COPY . /app/
RUN dotnet restore

COPY certs/ /certs/
RUN dotnet publish -c Release

EXPOSE 80
CMD ["dotnet", "watch", "--project", "/app/src", "run"]


FROM mcr.microsoft.com/dotnet/aspnet:5.0-rc2 AS production

WORKDIR /app
COPY --from=development /publish /app
COPY --from=development /certs /certs
COPY entrypoint.sh /

RUN  \
    apt-get update && \
    apt-get install -y python3 python3-pip && \
    pip3 install awscli && \
    chmod +x /entrypoint.sh

EXPOSE 80
CMD ["dotnet", "/app/Identity.dll"]

