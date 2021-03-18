# Generating public/private key pair

```
openssl genrsa -out norway_rsa.pem 1024
openssl rsa -in norway_rsa.pem -pubout > norway_rsa.pub
base64 < norway_rsa.pem
base64 < norway_rsa.pub
```

The public key should be handed to the authority and both the public and private key
should be stored in EVA (as a blob).