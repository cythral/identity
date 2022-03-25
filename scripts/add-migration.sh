#!/bin/bash

MigrationName=$1

dotnet ef migrations add $MigrationName \
    --project src/Database \
    --msbuildprojectextensionspath ./obj/Database