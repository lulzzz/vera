# Sweden Infrasec Integration

Check the *_docs* folder for diagrams and documentation provided by Infrasec

### 1. Certificates
- Two certificates are used, one the Infrasec station enrollment API and one for sending
  the Infrasec Receipt API
- See *SwedenAuditingSettings* and *InfrasecCertificateValidator*
#### 1.1. Generating the .pfx file from .pem
- Download [openssl](https://www.openssl.org/source/) based on your system
- Get the Infrasec .pem file you want to modify, you will also be asked to provide the key/password
- Run `openssl pkcs12 -export -in CERT_NAME.pem -inkey CERT_NAME.pem -out enrollclient.pfx`

### 2. Infrasec Unique Register Identity
- Unique number created by Infrasec based on data sent on the Enrollment Request.
- See _docs/iSecure CCU Register Enrollment API 1.07.pdf, Chapter 8.3 and related.
- Configurable settings are available in SwedenAuditingSettings.cs

#### 2.1. Overview of how the Unique Register Identity Number is created
```
[1]NB  [2]01    [3]00SCS    [4]0330   [5]001
|      |        |           |         |
|      |        |           |         |- (3) Register one on the rituals store 225
|      |        |           |------ (4) Example for your specific code for the 330 store on the Scotch and soda chain chain
|      |        |----------- (5) Example for your specific code for the Scotch and Soda chain
|      |--------------- (2) Always 01 - 01 is always used for the test ENV
|------------------- (2) Always NB, hardcoded in code
```
[1] Infrasec Api PartnerCode
- Setting: Hardcoded in code, it's always "NB", no changes/setup needed.

[2] InfrasecApi Pos AuthorityCode
- Setting: **Auditing:Sweden:InfrasecApi:PosAuthorityCode**
- Maximum 2 characters. Not sanitized, 01 was always used for the test-env

[3] Tenant Code
- Setting: **Auditing:Sweden:Tenant:Code**
- Maximum 5 characters. Sanitized => if shorter, it will be prefixed with 0. E.g. 243 will become 00243.

[4] Shop Number
- Setting: **Auditing:Sweden:Shop:Number**
- Maximum 5 characters. Sanitized => if shorter, it will be prefixed with 0.

[5] Register/Station ID
- Setting: Computed automatically based on the total number of Swedish stations.
- Maximum 3 characters. Sanitized => if shorter, it will be prefixed with 0.