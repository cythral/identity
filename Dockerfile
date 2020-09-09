FROM tlsprint/openssl:1.1.1g AS certificate-generation
WORKDIR /cert
COPY openssl.cnf /usr/local/ssl/openssl.cnf
RUN openssl req -x509 -nodes -newkey rsa:2048 -keyout rsa_private.pem -out rsa_cert.pem -subj "/CN=cythral.com"


FROM mcr.microsoft.com/dotnet/core/sdk:3.1.301 AS development
WORKDIR /app

COPY src/Docker.props /Directory.Build.props
COPY .editorconfig global.json nuget.config /
COPY src/Identity.csproj src/packages.lock.json /app/
RUN dotnet restore

COPY src/ /app/
COPY --from=certificate-generation /cert/rsa_cert.pem /content/x509/identity.pem
COPY --from=certificate-generation /cert/rsa_private.pem /keys/privkey.pem
RUN dotnet publish -c Release

EXPOSE 80
CMD ["dotnet", "watch", "run"]


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS production

WORKDIR /app
COPY --from=development /publish /app
COPY --from=development /content /content
COPY --from=development /keys /keys

EXPOSE 80
CMD ["dotnet", "/app/Identity.dll"]

