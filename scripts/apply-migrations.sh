#!/bin/bash

dotnet ef database update \
    --project src/Server \
    --runtime osx-x64 \
    --framework net7.0