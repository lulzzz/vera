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

        private static readonly SemaphoreSlim Semaphore = new(1, 1);
        
        public Setup(ChannelBase channel, Faker faker)
        {
            _channel = channel;
            _faker = faker;

            _runShortId = (DateTime.UtcNow.Ticks - new DateTime(2020, 1, 1).Ticks).ToString("x");

            RegisterClient = new RegisterService.RegisterServiceClient(channel);
            LoginClient = new LoginService.LoginServiceClient(channel); 
            AccountClient = new AccountService.AccountServiceClient(channel);
            TokenClient = new TokenService.TokenServiceClient(channel);
        }

        public async Task<SetupClient> CreateClient(AccountContext context)
        {
            await Semaphore.WaitAsync();
            
            var loginEntry = await CreateLoginIfNotExists(context.CompanyName);
            var (account, exists) = await CreateAccountIfNotExists(context, loginEntry.Token);

            Semaphore.Release();

            var client = new SetupClient(this, _channel, loginEntry.Token, account);

            if (!exists && context.Configuration.Any())
            {
                var accountConfigurationRequest = new AccountConfigurationRequest
                {
                    Id = account,
                    Fields = { context.Configuration }
                };

                await client.Account.CreateOrUpdateConfigurationAsync(accountConfigurationRequest,
                    client.AuthorizedMetadata);
            }

            return client;
        }

        public async Task<LoginEntry> CreateLoginIfNotExists(string companyName)
        {
            if (CompanyLoginTokens.TryGetValue(companyName, out var entry))
            {
                return entry;
            }
            
            var registerRequest = new RegisterRequest
            {
                Username = _faker.Internet.UserName(),
                Password = _faker.Internet.Password(),
                CompanyName = _runShortId + _faker.Company.CompanyName()
            };

            using var registerCall = RegisterClient.RegisterAsync(registerRequest);

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

        public Metadata CreateAuthorizedMetadata(string token)
        {
            return new()
            {
                {"authorization", $"bearer {token}"}
            };
        }

        public RegisterService.RegisterServiceClient RegisterClient { get; }
        public LoginService.LoginServiceClient LoginClient { get; }
        public AccountService.AccountServiceClient AccountClient { get; }
        public TokenService.TokenServiceClient TokenClient { get; }
    }
}