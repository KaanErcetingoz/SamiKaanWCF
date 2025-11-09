using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SamiKaanWCF.Business;
using SamiKaanWCF.Models;

namespace SamiKaanWCF
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly ExchangeRateBusinessLogic _businessLogic;

        public ExchangeRateService()
        {
            _businessLogic = new ExchangeRateBusinessLogic();
        }

        public ExchangeRateResponse GetCurrentExchangeRate(string currencyCode)
        {
            try
            {
                return _businessLogic.GetCurrentExchangeRate(currencyCode);
            }
            catch (Exception ex)
            {
                return new ExchangeRateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Service error: {ex.Message}"
                };
            }
        }

        public ExchangeRateResponse[] GetAllCurrentRates()
        {
            try
            {
                return _businessLogic.GetAllCurrentRates();
            }
            catch (Exception ex)
            {
                return new[] { new ExchangeRateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Service error: {ex.Message}"
                }};
            }
        }

        public ExchangeRateResponse GetHistoricalRate(string currencyCode, DateTime date)
        {
            try
            {
                return _businessLogic.GetHistoricalRate(currencyCode, date);
            }
            catch (Exception ex)
            {
                return new ExchangeRateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Service error: {ex.Message}"
                };
            }
        }
    }
}
