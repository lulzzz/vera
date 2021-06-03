using Fiskaly;
using Fiskaly.Client.Models;
using Fiskaly.Errors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static Vera.Germany.Constants;

namespace Vera.Germany.Fiskaly
{
    public class FiskalyClient : IFiskalyClient
    {
        private readonly string _fiskalyApiKey;
        private readonly string _fiskalyApiSecret;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };
        private readonly FiskalyHttpClient _client;

        public FiskalyClient(string fiskalyApiKey, string fiskalyApiSecret, string baseUrl)
        {
            _fiskalyApiKey = fiskalyApiKey;
            _fiskalyApiSecret = fiskalyApiSecret;

            _client = new FiskalyHttpClient(_fiskalyApiKey, _fiskalyApiSecret, baseUrl);
        }

        public string Authenticate()
        {
            var model = new
            {
                api_key = _fiskalyApiKey,
                api_secret = _fiskalyApiSecret
            };
            var requestBody = CreateRequestBody(model);
            var response = _client.Request(RequestMethod.POST, "auth", requestBody, null, null);
            var responseDictionary = ReadResponseToDictionary(response.Body);

            return (string)responseDictionary["access_token"];
        }

        public GetTssModelResponse GetTss(string tssId)
        {
            var model = new
            {
                tss_id = tssId
            };
            var requestBody = CreateRequestBody(model);
            var response = _client.Request(RequestMethod.GET, "tss", requestBody, null, null);

            if (response.Status == 404)
            {
                return null;
            }

            if (response.Status != 200)
            {
                throw new FiskalyException($"Cannot get tss, response: {EncodingHelper.Decode(response.Body)}");
            }

            var responseDictionary = ReadResponseToDictionary(response.Body);

            return DeserializeObject<List<GetTssModelResponse>>(responseDictionary["data"].ToString()).First();
        }

        public void CreateTss(CreateTssModel createTssModel)
        {
            var model = new
            {
                description = createTssModel.Description,
                state = createTssModel.TssState.ToString()
            };
            var requestBody = CreateRequestBody(model);
            var response = _client.Request(RequestMethod.GET, $"tss/{createTssModel.TssId}", requestBody, null, null);

            if (response.Status != 200)
            {
                throw new FiskalyException($"Tss: '{createTssModel.TssId}' cannot be created, response: '{EncodingHelper.Decode(response.Body)}'");
            }
        }

        public GetClientModelResponse GetClient(string clientId, string tssId)
        {
            FiskalyHttpResponse response;
            try
            {
                response = _client.Request(RequestMethod.GET, $"tss/{tssId}/client/{clientId}", null, null, null);
            }
            catch (FiskalyHttpError ex)
            {
                if (ex.Code == "E_CLIENT_NOT_FOUND")
                {
                    return null;
                }

                throw new FiskalyException("FiskalyClient exception", ex);
            }

            if (response.Status == 404)
            {
                return null;
            }

            if (response.Status != 200)
            {
                throw new FiskalyException($"Cannot get client, response: {EncodingHelper.Decode(response.Body)}");
            }

            var data = EncodingHelper.Decode(response.Body);

            return DeserializeObject<GetClientModelResponse>(data);
        }

        public void CreateClient(CreateClientModel createTssModel)
        {
            var model = new
            {
                serial_number = createTssModel.SerialNumber
            };
            var requestBody = CreateRequestBody(model);
            var response = _client.Request(RequestMethod.PUT,
                $"tss/{createTssModel.TssId}/client/{createTssModel.ClientId}", requestBody, null, null);

            if (response.Status != 200)
            {
                throw new FiskalyException($"Tss: '{createTssModel.TssId}' cannot be created, response: '{EncodingHelper.Decode(response.Body)}'");
            }
        }

        public TransactionModelResponse CreateTransaction(CreateTransactionModel createTransactionModel)
        {
            //create transaction
            var model = new
            {
                state = "ACTIVE",
                client_id = createTransactionModel.ClientId,
                schema = new
                {
                    standard_v1 = new
                    {
                        receipt = new
                        {
                            receipt_type = "RECEIPT",
                            amounts_per_vat_rate = createTransactionModel.Taxes
                                .Select(t => new
                                {
                                    vat_rate = t.taxCategory,
                                    amount = FormatAmount(t.grossTotal)
                                })
                                .ToArray(),
                            amounts_per_payment_type = createTransactionModel.Payments.Select(p => new
                            {
                                amount = FormatAmount(p.amount),
                                payment_type = p.isCash ? "CASH" : "NON_CASH",
                                currency_code = createTransactionModel.Currency
                            })
                        }
                    }
                }
            };
            var requestBody = CreateRequestBody(model);
            var response = _client.Request(RequestMethod.PUT,
                $"tss/{createTransactionModel.TssId}/tx/{createTransactionModel.TxId}", requestBody, null, null);

            if (response.Status != 200)
            {
                throw new FiskalyException($"Transaction for tss: '{createTransactionModel.TssId}' cannot be created, response: '{EncodingHelper.Decode(response.Body)}'");
            }

            var data = EncodingHelper.Decode(response.Body);
            var transactionModel = DeserializeObject<TransactionModelResponse>(data);

            //finish transaction
            var model2 = new
            {
                state = "FINISHED",
                client_id = createTransactionModel.ClientId,
                schema = new
                {
                    standard_v1 = new
                    {
                        receipt = new
                        {
                            receipt_type = "RECEIPT",
                            amounts_per_vat_rate = createTransactionModel.Taxes
                                .Select(t => new
                                {
                                    vat_rate = t.taxCategory,
                                    amount = FormatAmount(t.grossTotal)
                                })
                                .ToArray(),
                            amounts_per_payment_type = createTransactionModel.Payments.Select(p => new
                            {
                                amount = FormatAmount(p.amount),
                                payment_type = p.isCash ? "CASH" : "NON_CASH",
                                currency_code = createTransactionModel.Currency
                            })
                        }
                    }
                }
            };
            requestBody = CreateRequestBody(model2);
            response = _client.Request(
                RequestMethod.PUT,
                $"tss/{createTransactionModel.TssId}/tx/{createTransactionModel.TxId}",
                requestBody,
                null,
                new Dictionary<string, object>
                {
                    ["last_revision"] = transactionModel.LatestRevision
                });

            if (response.Status != 200)
            {
                throw new FiskalyException($"Transaction for tss: '{createTransactionModel.TssId}' cannot be finished, response: '{EncodingHelper.Decode(response.Body)}'");
            }

            data = EncodingHelper.Decode(response.Body);

            return DeserializeObject<TransactionModelResponse>(data);
        }

        private static Dictionary<string, object> ReadResponseToDictionary(byte[] responseBody) =>
            JsonConvert.DeserializeObject<Dictionary<string, object>>(EncodingHelper.Decode(responseBody));

        private T DeserializeObject<T>(string jsonData) =>
            JsonConvert.DeserializeObject<T>(jsonData, _serializerSettings);

        private static byte[] CreateRequestBody(object model) => EncodingHelper.Encode(JsonConvert.SerializeObject(model));

        private static string FormatAmount(decimal amount) => amount.ToString("0.00######", CultureInfo.InvariantCulture);
    }
}
