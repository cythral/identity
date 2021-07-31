ARG CONFIGURATION=Release
ARG AWSCLI_VERSION=2.2.1
ARG TARGET_FRAMEWORK
ARG ASPNETCORE_VERSION
ARG ASSEMBLY_NAME

FROM public.ecr.aws/cythral/unzip AS awscli
ARG AWSCLI_VERSION
WORKDIR /
ADD https://awscli.amazonaws.com/awscli-exe-linux-x86_64-${AWSCLI_VERSION}.zip ./awscli.zip
RUN unzip awscli.zip && \
    rm awscli.zip

FROM mcr.microsoft.com/dotnet/aspnet:${ASPNETCORE_VERSION}
ARG PROJECT_DIRECTORY
ARG OUTPUT_DIRECTORY
ARG CONFIGURATION
ARG TARGET_FRAMEWORK
ARG ASSEMBLY_NAME

WORKDIR /app
ENV \
    CONFIGURATION=${CONFIGURATION} \
    DLL_PATH=/app/${ASSEMBLY_NAME}.dll

COPY --from=awscli /aws /usr/local/share/aws
COPY ./bin/Server/${CONFIGURATION}/${TARGET_FRAMEWORK}/publish /app
COPY ./bin/Interface/${CONFIGURATION}/${TARGET_FRAMEWORK}/publish/wwwroot /app/wwwroot
COPY ./entrypoint.sh /

RUN \
    /usr/local/share/aws/install && \
    chmod +x /entrypoint.sh

EXPOSE 80
ENTRYPOINT /entrypoint.sh

