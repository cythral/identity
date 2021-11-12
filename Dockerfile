ARG CONFIGURATION=Release

FROM public.ecr.aws/cythral/brighid/base:0.1.10
ARG PROJECT_DIRECTORY
ARG OUTPUT_DIRECTORY
ARG CONFIGURATION
ARG TARGET_FRAMEWORK

EXPOSE 80
WORKDIR /app
ENV CONFIGURATION=${CONFIGURATION}

COPY ./entrypoint.sh /
COPY ./bin/Server/${CONFIGURATION}/${TARGET_FRAMEWORK}/linux-musl-x64/publish /app/
COPY ./bin/Interface/${CONFIGURATION}/${TARGET_FRAMEWORK}/publish/wwwroot /wwwroot

RUN setcap 'cap_net_bind_service=+ep' /app/Server

ENTRYPOINT [ "/entrypoint.sh" ]

