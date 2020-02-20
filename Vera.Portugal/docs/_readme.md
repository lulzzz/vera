# SAFT

## Initial setup

It is important that all settings are initially correct, as they will lock into chains and hashes for all eternity. If the first few orders were shipped with an invalid shop VAT number, it can not be fixed afterwards and these invoices will always remain erroreous.

Use the services `AuditingValidateConfiguration` and `AuditingExecuteConfiguration` to help you validate and set up the initial configuration. The first service will check for required properties, settings and open financial periods, while the second will actually configure the `Auditing:Provider` setting and mark all previous financial periods as 'processed', so you can start selling!

## Requirements

### Organization Units

Configure a Country type OU for the country you are setting up for auditing. All settings for auditing should end up on this OU, and all shops should be children of this OU (at some level).

All the shops for this country should have the following properties filled in;

* `Name`
* `VatNumber` (exported as `CompanyID` and `TaxRegistrationNumber`)
* `Address.Street`
* `Address.ZipCode`
* `Address.City`
* `Address.CountryID`

### PDF renderer

SAFT requires page number headers in the Invoice Stencil layout, and thus the PDF renderer is required to support this functionality. Note that this backend application is not yet fully battle-tested, and needs to be configured for your specific envirnoment bij @infra.

## Configuration

### Settings

The following settings are required;

| Setting                              | Value                            |
| ------------------------------------ | -------------------------------- |
| `Auditing:Provider`                  | `SAFT`                           |
| `Auditing:PrivateKeyBlobID`          | (see below)                      |
| `Auditing:CertificateName`           | `PELICANTHEORY - UNIPESSOAL LDA` |
| `Auditing:CertificateNumber`         | `2741`                           |
| `Auditing:DuplicatePrint`            | `true`                           |
| `App:Customer:ShowFiscalID`          | `true`                           |
| `App:Order:ShowFiscalID`             | `true`                           |
| `UniqueInvoiceNumberSequence`        | `true`                           |
| `UniqueInvoiceNumberSequencePerType` | `true`                           |

For user registration, it is important, as customer, to be able to register `User.FiscalID` ("NIF") through the app. This feeds into a lottery at state level once the data is collected, rewarding loyal taxpayers with a chance to win prizes every week.

### Keys

For SAFT, only the private key is used and should be configured in `Auditing:PrivateKeyBlobID`. You'll find `001_eva_private.pem` stored with this document in our private repository for initial configuration.

You can upload the key using the `AuditingSetPrivateKey` service. This service will automatically create the blob and store it's key in the setting, and will increment the `Auditing:Version` setting too. Just note that it will increase the default value as well, so a first initial upload of a key will result in a version value of 2 - you may want to fix that, for sanity reasons.

### Tasks

The `CreateFinancialPeriodAuditTask` can be configured, but is optional. This task will basically do an automated generation of an audit file, which you can also trigger manually through the admin. It would be recommended to schedule this task monthly, perhaps the 2nd of each month, so the chunks to audit are left relatively small.

### Stencils

Please configure the Stencil template according to requirements, and confirm with the customer and our compliance officer. In the near future, we will serve an unmodifiable, static EVA template that adheres to all compliance requirements and allows for simple customer customization. For now, this is a manual task.
