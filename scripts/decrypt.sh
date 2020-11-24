#!/bin/bash
set -eo pipefail
ciphertext=$1
tempfile=$(mktemp)

echo $ciphertext | base64 --decode > $tempfile
echo $(aws kms decrypt --ciphertext-blob fileb://$tempfile --query Plaintext --output text | base64 --decode)
rm $tempfile;
