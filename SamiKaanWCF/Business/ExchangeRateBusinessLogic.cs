// ====================================================================
// FILE: Business/ExchangeRateBusinessLogic.cs (Synchronous version for WCF)
// PURPOSE: Business logic for exchange rate operations using NBP API
// ====================================================================

using System;
using System.Linq;
using SamiKaanWCF.Models;
using SamiKaanWCF.Services;

namespace SamiKaanWCF.Business
{
    public class ExchangeRateBusinessLogic
    {
        private readonly NBPApiClient _nbpClient;
        private readonly string[] _supportedCurrencies = {
            "USD", "EUR", "GBP", "CHF", "JPY", "CAD", "AUD",
            "SEK", "NOK", "DKK", "CZK", "HUF", "RUB", "UAH"
        };

        public ExchangeRateBusinessLogic()
        {
            _nbpClient = new NBPApiClient();
        }

        public ExchangeRateResponse GetCurrentExchangeRate(string currencyCode)
        {
            if (!ValidateCurrencyCode(currencyCode))
            {
                return new ExchangeRateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid currency code or currency not supported"
                };
            }

            try
            {
                var nbpResponse = _nbpClient.GetCurrentRate(currencyCode.ToUpper());

                if (nbpResponse?.rates?.FirstOrDefault() != null)
                {
                    var rate = nbpResponse.rates.First();
                    return new ExchangeRateResponse
                    {
                        Currency = currencyCode.ToUpper(),
                        Rate = rate.mid,
                        EffectiveDate = rate.effectiveDate.ToString("yyyy-MM-dd"),
                        TableType = nbpResponse.table,
                        IsSuccess = true
                    };
                }
                else
                {
                    return new ExchangeRateResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Currency rate not found"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ExchangeRateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error retrieving exchange rate: {ex.Message}"
                };
            }
        }

        public ExchangeRateResponse[] GetAllCurrentRates()
        {
            try
            {
                var nbpResponse = _nbpClient.GetAllCurrentRates();

                if (nbpResponse?.rates != null)
                {
                    return nbpResponse.rates.Select(rate => new ExchangeRateResponse
                    {
                        Currency = rate.code,
                        Rate = rate.mid,
                        EffectiveDate = nbpResponse.effectiveDate.ToString("yyyy-MM-dd"),
                        TableType = nbpResponse.table,
                        IsSuccess = true
                    }).ToArray();
                }
                else
                {
                    return new[] { new ExchangeRateResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Unable to retrieve current exchange rates"
                    }};
                }
            }
            catch (Exception ex)
            {
                return new[] { new ExchangeRateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error retrieving exchange rates: {ex.Message}"
                }};
            }
        }

        public ExchangeRateResponse GetHistoricalRate(string currencyCode, DateTime date)
        {
            if (!ValidateCurrencyCode(currencyCode))
            {
                return new ExchangeRateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid currency code or currency not supported"
                };
            }

            if (date > DateTime.Now.Date)
            {
                return new ExchangeRateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Cannot retrieve future exchange rates"
                };
            }

            try
            {
                var nbpResponse = _nbpClient.GetHistoricalRate(currencyCode.ToUpper(), date);

                if (nbpResponse?.rates?.FirstOrDefault() != null)
                {
                    var rate = nbpResponse.rates.First();
                    return new ExchangeRateResponse
                    {
                        Currency = currencyCode.ToUpper(),
                        Rate = rate.mid,
                        EffectiveDate = rate.effectiveDate.ToString("yyyy-MM-dd"),
                        TableType = nbpResponse.table,
                        IsSuccess = true
                    };
                }
                else
                {
                    return new ExchangeRateResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Historical rate not found for the specified date"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ExchangeRateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error retrieving historical rate: {ex.Message}"
                };
            }
        }

        private bool ValidateCurrencyCode(string currencyCode)
        {
            return !string.IsNullOrEmpty(currencyCode) &&
                   currencyCode.Length == 3 &&
                   _supportedCurrencies.Contains(currencyCode.ToUpper());
        }
    }
}