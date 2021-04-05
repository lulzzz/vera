using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Grpc.Core;
using Vera.Grpc;
using Vera.Grpc.Shared;

namespace Vera.Integration.Tests
{
    public class AccountContext
    {
        public string CompanyName { get; set; }
        public string AccountName { get; set; }
        public string Certification { get; set; }
        public IDictionary<string, string> Configuration { get; } = new Dictionary<string, string>();
        public string SupplierSystemId { get; set; }
    }

    public class LoginEntry
    {
        public string Username { get; set; }
        public string Token { get; set; }
    }
    
    public class Setup
    {
        private readonly Faker _faker;
        private readonly ChannelBase _channel;
        
        // Used to prefix certain values to prevent "already exists" errors if running multiple times
        // against the same database without cleaning up
        private readonly string _runShortId;

        private static readonly IDictionary<string, LoginEntry> CompanyLoginTokens =
            new ConcurrentDictionary<string, LoginEntry>();
        
        // Cache of accounts that are created during tests
        private static readonly IDictionary<string, string> ExistingAccounts =
            new ConcurrentDictionary<string, string>();

        private static readonly IDictionary<string, string> ExistingSuppliers =
            new ConcurrentDictionary<string, string>();

        private static readonly SemaphoreSlim Semaphore = new(1, 1);
        
        public Setup(ChannelBase channel, Faker faker)
        {
            _channel = channel;
            _faker = faker;

            _runShortId = (DateTime.UtcNow.Ticks - new DateTime(2020, 1, 1).Ticks).ToString("x");

            RegisterClient = new UserRegisterService.UserRegisterServiceClient(channel);
            LoginClient = new LoginService.LoginServiceClient(channel); 
            AccountClient = new AccountService.AccountServiceClient(channel);
            TokenClient = new TokenService.TokenServiceClient(channel);
        }

        public async Task<SetupClient> CreateClient(AccountContext context)
        {
            await Semaphore.WaitAsync();

            try
            {
                var loginEntry = await CreateLoginIfNotExists(context.CompanyName);
                var (account, exists) = await CreateAccountIfNotExists(context, loginEntry.Token);

                var client = new SetupClient(this, _channel, loginEntry.Token, account);

                if (!string.IsNullOrEmpty(context.SupplierSystemId))
                {
                    client.SupplierSystemId = await CreateSupplierIfNotExists(context, client);
                }
                
                if (exists)
                {
                    // Nothing else to configure or client exists already
                    return client;
                }

                if (context.Configuration.Any())
                {
                    var accountConfigurationRequest = new AccountConfigurationRequest
                    {
                        Id = account,
                        Fields = {context.Configuration}
                    };

                    await client.Account.CreateOrUpdateConfigurationAsync(accountConfigurationRequest,
                        client.AuthorizedMetadata);                    
                }

                return client;
            }
            finally
            {
                Semaphore.Release();    
            }
        }
        
        public async Task<LoginEntry> CreateLoginIfNotExists(string companyName)
        {
            if (CompanyLoginTokens.TryGetValue(companyName, out var entry))
            {
                return entry;
            }
            
            var registerRequest = new RegisterUserRequest
            {
                Username = _faker.Internet.UserName(),
                Password = _faker.Internet.Password(),
                CompanyName = _runShortId + _faker.Company.CompanyName()
            };

            using var registerCall = RegisterClient.RegisterUserAsync(registerRequest);

            await registerCall.ResponseAsync;

            using var loginCall = LoginClient.LoginAsync(new LoginRequest
            {
                Username = registerRequest.Username,
                Password = registerRequest.Password,
                CompanyName = registerRequest.CompanyName
            });

            var loginResponse = await loginCall.ResponseAsync;

            entry = new LoginEntry
            {
                Username = registerRequest.Username,
                Token = loginResponse.Token
            };

            CompanyLoginTokens[companyName] = entry;
            
            return entry;
        }

        private async Task<(string, bool)> CreateAccountIfNotExists(AccountContext context, string token)
        {
            if (ExistingAccounts.TryGetValue(context.AccountName, out var accountId))
            {
                return (accountId, true);
            }
            
            var address = _faker.Address;

            var accountToCreate = new CreateAccountRequest
            {
                Name = context.AccountName,
                Certification = context.Certification,
                Address = new Address
                {
                    City = address.City(),
                    Country = address.CountryCode(),
                    Number = address.BuildingNumber(),
                    Region = address.StateAbbr(),
                    PostalCode = address.ZipCode(),
                    Street = address.StreetName()
                }
            };

            using var createAccountCall = AccountClient.CreateAsync(accountToCreate, CreateAuthorizedMetadata(token));
            
            var createAccountReply = await createAccountCall.ResponseAsync;

            ExistingAccounts[context.AccountName] = createAccountReply.Id;

            return (createAccountReply.Id, false);
        }

        private async Task<string> CreateSupplierIfNotExists(AccountContext context, SetupClient client)
        {
            if (ExistingSuppliers.TryGetValue(context.SupplierSystemId, out var s))
            {
                return s;
            }
            
            var supplier = new CreateSupplierRequest
            {
                Supplier = new Supplier
                {
                    Name = _faker.Company.CompanyName(),
                    RegistrationNumber = _faker.Random.AlphaNumeric(10),
                    TaxRegistrationNumber = _faker.Random.AlphaNumeric(10),
                    SystemId = context.SupplierSystemId,
                    Address = new Address
                    {
                        City = _faker.Address.City(),
                        Country = _faker.Address.Country(),
                        Number = _faker.Address.BuildingNumber(),
                        PostalCode = _faker.Address.ZipCode(),
                        Region = _faker.Address.County(),
                        Street = _faker.Address.StreetAddress()
                    }
                }
            };

            await client.Supplier.CreateIfNotExistsAsync(supplier, client.AuthorizedMetadata);

            // TODO(kevin): convert to set, we can use SystemId instead of Id anyway
            ExistingSuppliers[context.SupplierSystemId] = context.SupplierSystemId;
            
            return context.SupplierSystemId;
        }
        
        public Metadata CreateAuthorizedMetadata(string token)
        {
            return new()
            {
                {"authorization", $"bearer {token}"}
            };
        }

        public UserRegisterService.UserRegisterServiceClient RegisterClient { get; }
        public LoginService.LoginServiceClient LoginClient { get; }
        public AccountService.AccountServiceClient AccountClient { get; }
        public TokenService.TokenServiceClient TokenClient { get; }
    }
}