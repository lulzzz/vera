# "Vera"
Latin for true. We want this product to be the one and only source for all the auditing and certification needs.

# Structure of the projects
- Vera
  - Where the core magic happens of Vera. All the generic steps e.d are defined in here
- Vera.Bootstrap
  - The "glue" between the core of Vera and all the specific certification implementations
- Vera.Host
  - Use the components of Vera to expose a gRPC API to allow other parties to make use of the
    internal certification capabilities that Vera offers
- Vera.Documents
  - Houses the components that can be shared with other C# projects on how to interpret documents
- Vera.[Country] (replace Country with Portugal, Norway, etc)
  - These projects house the specific implementations that are required to make Vera comply with
    the regulations of that country

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
- Containers
  - companies
    - Container for: companies, accounts and user
    - Partitioned on the id of the company
  - invoices
    - Container for: invoices
    - Partitioned on the "bucket" that the invoice belongs to, its chain essentially
    - Also partioned on account + invoice number to allow efficient fetching on the invoice number
  - audits
    - Container for: audits

