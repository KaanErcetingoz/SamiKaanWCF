using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SamiKaanWCF.Models;

namespace SamiKaanWCF.Services
{
    public class NBPApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://api.nbp.pl/api/exchangerates/";

        public NBPApiClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CurrencyExchangeApp/1.0");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public NBPRateResponse GetCurrentRate(string currencyCode)
        {
            try
            {
                string url = $"{BaseUrl}rates/a/{currencyCode}/";
                var response = _httpClient.GetStringAsync(url).Result;
                return JsonConvert.DeserializeObject<NBPRateResponse>(response);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public NBPTableResponse GetAllCurrentRates()
        {
            try
            {
                string url = $"{BaseUrl}tables/a/";
                var response = _httpClient.GetStringAsync(url).Result;
                var tables = JsonConvert.DeserializeObject<NBPTableResponse[]>(response);
                return tables?[0];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public NBPRateResponse GetHistoricalRate(string currencyCode, DateTime date)
        {
            try
            {
                string dateStr = date.ToString("yyyy-MM-dd");
                string url = $"{BaseUrl}rates/a/{currencyCode}/{dateStr}/";
                var response = _httpClient.GetStringAsync(url).Result;
                return JsonConvert.DeserializeObject<NBPRateResponse>(response);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}