#!/bin/sh

set -eo pipefail

if [ "$Environment" != "local" ]; then
    export Database__Password=$(decrs ${Encrypted__Database__Password});
    runuser --user brighid /app/Server
    exit $?
fi

watch /app "runuser --user brighid /app/Server"