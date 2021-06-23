#!/bin/bash

decrypt() {
    ciphertext=$1
    tempfile=$(mktemp)

    echo $ciphertext | base64 --decode > $tempfile
    echo $(aws kms decrypt --ciphertext-blob fileb://$tempfile --query Plaintext --output text | base64 --decode)
    rm $tempfile;
}

tag_image() {
    TAG=$1
    IFS=':' read -ra IMAGE_PARTS <<< "$App__Image"
    IFS='/' read -ra REPOSITORY_PARTS <<< "${IMAGE_PARTS[0]}"
    REPOSITORY_NAME=${REPOSITORY_PARTS[1]}
    TAG_NAME=${IMAGE_PARTS[1]}

    MANIFEST=$(aws ecr batch-get-image --repository-name $REPOSITORY_NAME --image-ids imageTag=$TAG_NAME --query 'images[].imageManifest' --output text)
    aws ecr put-image --repository-name $REPOSITORY_NAME --image-tag $TAG --image-manifest "$MANIFEST" >/dev/null || return 0
}

export Database__Password=$(decrypt ${Encrypted__Database__Password})

tag_image $Environment__Name
dotnet /app/Server.dll