ARG CONFIGURATION=Release

FROM public.ecr.aws/cythral/brighid/base:0.1.13
ARG CONFIGURATION

EXPOSE 80
WORKDIR /app
ENV CONFIGURATION=${CONFIGURATION}

COPY ./entrypoint.sh /
COPY ./bin/Server/${CONFIGURATION}/linux-musl-x64/publish /app/
COPY ./bin/Interface/${CONFIGURATION}/publish/wwwroot /wwwroot

RUN setcap 'cap_net_bind_service=+ep' /app/Server

ENTRYPOINT [ "/entrypoint.sh" ]

