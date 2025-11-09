// ===================================================================
// FINAL CORRECTED WcfService.cs
//
// FIX: All service call URLs now include the "/rest/" path to match
// the web.config and target the correct JSON endpoint.
// ===================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CurrencyExchangeMobile.Services
{
    public class WcfService : IDisposable
    {
        private readonly HttpClient _httpClient;

        private static string GetBaseUrl()
        {
#if ANDROID
            return "http://10.0.2.2:60729";
#else
            return "http://localhost:60729";
#endif
        }

        public static string CurrentUserId { get; set; }
        public static string CurrentUsername { get; set; }
        public static decimal CurrentBalance { get; set; }

        public WcfService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // ===================================================================
        // USER AUTHENTICATION
        // ===================================================================

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var request = new { Username = username, Email = email, Password = password };
                // FIX: Added "/rest/" to the URL path
                var response = await PostAsync($"{GetBaseUrl()}/UserService.svc/rest/CreateAccount", request);
                if (response is JsonElement json && json.TryGetProperty("IsSuccess", out var success))
                {
                    return success.GetBoolean();
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Registration error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var request = new { Username = username, Password = password };
                // FIX: Added "/rest/" to the URL path
                var response = await PostAsync($"{GetBaseUrl()}/UserService.svc/rest/AuthenticateUser", request);

                if (response is JsonElement json && json.TryGetProperty("IsSuccess", out var success) && success.GetBoolean())
                {
                    if (json.TryGetProperty("User", out var userElement))
                    {
                        CurrentUserId = userElement.GetProperty("UserId").GetString();
                        CurrentUsername = userElement.GetProperty("Username").GetString();
                        CurrentBalance = userElement.GetProperty("PLNBalance").GetDecimal();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login error: {ex.Message}");
                return false;
            }
        }

        // ===================================================================
        // ACCOUNT MANAGEMENT
        // ===================================================================

        public async Task<bool> TopUpAccountAsync(decimal amount)
        {
            try
            {
                var request = new { UserId = CurrentUserId, Amount = amount, TransferReference = $"TOPUP-{DateTime.Now:yyyyMMddHHmmss}" };
                // FIX: Added "/rest/" to the URL path
                var response = await PostAsync($"{GetBaseUrl()}/UserService.svc/rest/TopUpAccount", request);
                if (response is JsonElement json && json.TryGetProperty("IsSuccess", out var successElem) && successElem.GetBoolean())
                {
                    if (json.TryGetProperty("NewPLNBalance", out var balanceElem))
                    {
                        CurrentBalance = balanceElem.GetDecimal();
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Top up error: {ex.Message}");
                return false;
            }
        }

        public async Task<decimal> GetCurrentBalanceAsync()
        {
            try
            {
                // FIX: Added "/rest/" to the URL path
                var response = await PostAsync($"{GetBaseUrl()}/UserService.svc/rest/GetUserBalance", CurrentUserId);
                if (response is JsonElement json && json.TryGetProperty("PLNBalance", out var balanceElem))
                {
                    CurrentBalance = balanceElem.GetDecimal();
                }
                return CurrentBalance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Get balance error: {ex.Message}");
                return CurrentBalance; // Return cached value on error
            }
        }

        // ===================================================================
        // EXCHANGE RATES
        // ===================================================================

        public async Task<List<ExchangeRate>> GetCurrentRatesAsync()
        {
            try
            {
                // FIX: Added "/rest/" to the URL path
                var response = await PostAsync($"{GetBaseUrl()}/ExchangeRateService.svc/rest/GetAllCurrentRates", new { });
                if (response is JsonElement json && json.ValueKind == JsonValueKind.Array)
                {
                    return json.EnumerateArray()
                        .Select(rateElem => new ExchangeRate
                        {
                            Currency = rateElem.GetProperty("Currency").GetString(),
                            Rate = rateElem.GetProperty("Rate").GetDecimal(),
                            EffectiveDate = rateElem.TryGetProperty("EffectiveDate", out var d) ? d.GetString() : "N/A",
                            TableType = rateElem.TryGetProperty("TableType", out var t) ? t.GetString() : "A"
                        }).ToList();
                }
                return new List<ExchangeRate>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Get rates error: {ex.Message}");
                return new List<ExchangeRate>();
            }
        }

        public async Task<ExchangeRate> GetRateForCurrencyAsync(string currencyCode)
        {
            try
            {
                var request = new { CurrencyCode = currencyCode };
                // FIX: Added "/rest/" to the URL path
                var response = await PostAsync($"{GetBaseUrl()}/ExchangeRateService.svc/rest/GetCurrentRate", request);
                if (response is JsonElement json)
                {
                    return new ExchangeRate
                    {
                        Currency = json.GetProperty("Currency").GetString(),
                        Rate = json.GetProperty("Rate").GetDecimal(),
                        EffectiveDate = json.TryGetProperty("EffectiveDate", out var d) ? d.GetString() : "N/A",
                        TableType = json.TryGetProperty("TableType", out var t) ? t.GetString() : "A"
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Get rate for currency error: {ex.Message}");
                return null;
            }
        }

        // ===================================================================
        // CURRENCY TRANSACTIONS
        // ===================================================================

        public async Task<decimal?> CalculateBuyPriceAsync(string currencyCode, decimal amount)
        {
            try
            {
                var request = new { CurrencyCode = currencyCode, Amount = amount };
                // FIX: Added "/rest/" to the URL path
                var response = await PostAsync($"{GetBaseUrl()}/CurrencyTransactionService.svc/rest/CalculateBuyPrice", request);
                if (response is JsonElement json && json.TryGetProperty("TotalPrice", out var price))
                {
                    return price.GetDecimal();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Calculate buy price error: {ex.Message}");
                return null;
            }
        }

        public async Task<decimal?> CalculateSellPriceAsync(string currencyCode, decimal amount)
        {
            try
            {
                var request = new { CurrencyCode = currencyCode, Amount = amount };
                // FIX: Added "/rest/" to the URL path
                var response = await PostAsync($"{GetBaseUrl()}/CurrencyTransactionService.svc/rest/CalculateSellPrice", request);
                if (response is JsonElement json && json.TryGetProperty("TotalPrice", out var price))
                {
                    return price.GetDecimal();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Calculate sell price error: {ex.Message}");
                return null;
            }
        }

        public async Task<TransactionResult> BuyCurrencyAsync(string currencyCode, decimal amount)
        {
            try
            {
                var request = new { UserId = CurrentUserId, CurrencyCode = currencyCode, Amount = amount };
                // FIX: Added "/rest/" to the URL path
                var response = await PostAsync($"{GetBaseUrl()}/CurrencyTransactionService.svc/rest/BuyCurrency", request);
                if (response is JsonElement json)
                {
                    var result = new TransactionResult
                    {
                        IsSuccess = json.GetProperty("IsSuccess").GetBoolean(),
                        Message = json.TryGetProperty("Message", out var msg) ? msg.GetString() : ""
                    };
                    if (result.IsSuccess && json.TryGetProperty("NewPLNBalance", out var bal))
                    {
                        result.NewBalance = bal.GetDecimal();
                        CurrentBalance = result.NewBalance;
                    }
                    return result;
                }
                return new TransactionResult { IsSuccess = false, Message = "Server response was invalid." };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Buy currency error: {ex.Message}");
                return new TransactionResult { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<TransactionResult> SellCurrencyAsync(string currencyCode, decimal amount)
        {
            try
            {
                var request = new { UserId = CurrentUserId, CurrencyCode = currencyCode, Amount = amount };
                // FIX: Added "/rest/" to the URL path
                var response = await PostAsync($"{GetBaseUrl()}/CurrencyTransactionService.svc/rest/SellCurrency", request);
                if (response is JsonElement json)
                {
                    var result = new TransactionResult
                    {
                        IsSuccess = json.GetProperty("IsSuccess").GetBoolean(),
                        Message = json.TryGetProperty("Message", out var msg) ? msg.GetString() : ""
                    };
                    if (result.IsSuccess && json.TryGetProperty("NewPLNBalance", out var bal))
                    {
                        result.NewBalance = bal.GetDecimal();
                        CurrentBalance = result.NewBalance;
                    }
                    return result;
                }
                return new TransactionResult { IsSuccess = false, Message = "Server response was invalid." };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Sell currency error: {ex.Message}");
                return new TransactionResult { IsSuccess = false, Message = ex.Message };
            }
        }

        // ===================================================================
        // HTTP HELPER
        // ===================================================================

        private async Task<object> PostAsync(string url, object data)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(data, options);

                // For a bare string, don't wrap in quotes
                if (data is string)
                {
                    json = data.ToString();
                }

                Console.WriteLine($"🔗 Trying to connect to: {url}");
                Console.WriteLine($"📤 Sending: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"📡 Response Status: {response.StatusCode}");
                Console.WriteLine($"📥 Response: {responseJson}");

                if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(responseJson))
                {
                    var trimmed = responseJson.TrimStart();
                    // XML response detected -> convert to JSON-equivalent object
                    if (trimmed.StartsWith("<"))
                    {
                        var xdoc = XDocument.Parse(responseJson);
                        var ns = xdoc.Root.GetDefaultNamespace();

                        var items = xdoc
                            .Descendants(ns + "ExchangeRateResponse")
                            .Select(x =>
                            {
                                var rateStr = (string?)x.Element(ns + "Rate") ?? (string?)x.Element("Rate") ?? "0";
                                decimal rateVal = 0m;
                                decimal.TryParse(rateStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out rateVal);

                                return new
                                {
                                    Currency = (string?)x.Element(ns + "Currency") ?? (string?)x.Element("Currency") ?? "",
                                    Rate = rateVal,
                                    EffectiveDate = (string?)x.Element(ns + "EffectiveDate") ?? (string?)x.Element("EffectiveDate") ?? "",
                                    TableType = (string?)x.Element(ns + "TableType") ?? (string?)x.Element("TableType") ?? "A",
                                    IsSuccess = bool.TryParse((string?)x.Element(ns + "IsSuccess") ?? (string?)x.Element("IsSuccess"), out var b) ? b : true
                                };
                            })
                            .ToList();

                        var xmlAsJson = JsonSerializer.Serialize(items, options);
                        return JsonSerializer.Deserialize<object>(xmlAsJson)!;
                    }

                    return JsonSerializer.Deserialize<object>(responseJson);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Connection Error: {ex.GetType().Name} - {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // ===================================================================
    // MODELS (These should be in separate files for better organization)
    // ===================================================================

    public class ExchangeRate
    {
        public string Currency { get; set; }
        public decimal Rate { get; set; }
        public string EffectiveDate { get; set; }
        public string TableType { get; set; }
    }

    public class TransactionResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public decimal NewBalance { get; set; }
    }
}
