#!/bin/bash

MigrationName=$1

dotnet ef migrations add $MigrationName \
    --project src/Server \
    --runtime osx-x64 \
    --msbuildprojectextensionspath ./obj/Server