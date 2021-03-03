# "Vera"
Latin for true. We want this product to be the one and only source for all the auditing and certification needs.

# Structure of the projects
- Vera
  - Where the core magic happens of Vera. All the generic steps e.d are defined in here
- Vera.Bootstrap
  - The "glue" between the core of Vera and all the specific certification implementations
- Vera.Documents
  - Houses the components that can be shared with other C# projects on how to interpret documents
- Vera.Azure
  - All of the Azure specific implementations of the components that are used
- Vera.Grpc
  - Grpc definitions of the services, also packaged to be used by external parties to integrate with Vera
- Vera.[Country] (replace Country with Portugal, Norway, etc)
  - These projects house the specific implementations that are required to make Vera comply with
    the regulations of that country
- Vera.Host
  - Use the components of Vera to expose a gRPC API to allow other parties to make use of the
    internal certification capabilities that Vera offers

# Technologies used
- ASP.NET core for hosting
- gRPC for the communication between server <-> client
- Makes extensive use of Azure Cosmos DB as the database (https://docs.microsoft.com/en-us/azure/cosmos-db)
- Azure blobs are used for locking and storing files

# Development
- Latest dotnet core version (https://dotnet.microsoft.com/download)
- Currently requires access to a Windows machine/vm to be able to run the Cosmos emulator (https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator)
- Either Rider or Visual Studio (prefer Rider :))

## Registering a new service
- Invoke the `.MapGrpcService<>(..)` on the `endpoints` in the `Startup.cs` in the Vera.Host project

## Conventions
- Use lowercase strings for logging, error messages, etc.
- `if` can go on 1 line if it's just 1 line

## Cosmos model
- companies
  - Container for: `Company`, `Account` and `User`
  - All of these are partitioned on the id of the `Company`, separated by the `Type` property
- invoices
  - Container for: `Invoice`
  - Partioned on id of the account that created the invoice plus the invoice' number
- audits
  - Container for: `Audit`
  - Partitioned on the id of the account
- trails
  - Container for: `PrintAuditTrail`
  - Partitioned on the id of the invoice
- chains
  - Container for: `ChainDocument`
  - Partitioned on the "bucket" which is defined by the caller, for invoices this is defined by its "bucket"

# Deployment

## Configuration (environment variables)

### Cosmos
- `VERA__COSMOS__CONNECTIONSTRING` (required)
  - Connection string to use to connect to the Cosmos instance
- `VERA__COSMOS__DATABASE`
  - Name of the database to use in the Cosmos
  - Defaults to "vera"
- `VERA__COSMOS__CONTAINER__INVOICES`
  - Name of the container to use for the invoices
  - Defaults to "invoices"
- `VERA__COSMOS__CONTAINER__COMPANIES`
  - Name of the container to use for the companies, accounts and users
  - Defaults to "companies"
- `VERA__COSMOS__CONTAINER__AUDITS`
  - Name of the container to use for the audits
  - Defaults to "audits"
- `VERA__COSMOS__CONTAINER__TRAILS`
  - Name of the container to use for trails like printing
  - Defaults to "trails"

### Blobs
- `VERA__BLOB__CONNECTIONSTRING`
  - Connection string to use to connect to the Azure blob storage
  - If this is not configured, Vera will fallback to temporary storage (you don't want this in production)

### JWT
- `VERA__JWT__ISSUER`
  - Issuer to set in the JWT
- `VERA__JWT__KEY`
  - Symmetric key to use to sign the JWT with (base64 format)