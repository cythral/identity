#!/bin/sh

set -eo pipefail

if [ "$Environment" != "local" ]; then
    export Database__Password=$(decrs ${Encrypted__Database__Password}) || exit 1;
    runuser --user brighid /app/Server
    exit $?
fi

watch /app "runuser --user brighid /app/Server"