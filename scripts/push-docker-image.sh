#!/bin/bash

set -eo pipefail

if [ "$CODEBUILD_GIT_BRANCH" = "master" ]; then
    docker push $1
fi