ARG CONFIGURATION=Release

FROM public.ecr.aws/cythral/brighid/base:0.4.0.122
ARG CONFIGURATION

EXPOSE 80
WORKDIR /app
ENV CONFIGURATION=${CONFIGURATION}

COPY ./entrypoint.sh /
COPY ./bin/Server/${CONFIGURATION}/linux-musl-arm64/publish /app/
COPY ./bin/Interface/${CONFIGURATION}/publish/wwwroot /wwwroot

RUN setcap 'cap_net_bind_service=+ep' /app/Server

ENTRYPOINT [ "/entrypoint.sh" ]

