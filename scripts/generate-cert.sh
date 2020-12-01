#!/bin/bash

cwd=$(dirname ${BASH_SOURCE[0]})
mkdir -p $cwd/.cert
openssl req -x509 -days 90 -nodes -newkey rsa:4096 -keyout $cwd/.cert/rsa_private.pem -out $cwd/.cert/rsa_cert.pem -subj "/CN=cythral.com"
openssl pkcs12 -export -in $cwd/.cert/rsa_cert.pem -inkey $cwd/.cert/rsa_private.pem -out $cwd/../certs/cert.pfx
