using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SamiKaanWCF.Models;

namespace SamiKaanWCF
{
    [ServiceContract]
    public interface IExchangeRateService
    {
        [OperationContract]
        ExchangeRateResponse GetCurrentExchangeRate(string currencyCode);

        [OperationContract]
        ExchangeRateResponse[] GetAllCurrentRates();

        [OperationContract]
        ExchangeRateResponse GetHistoricalRate(string currencyCode, DateTime date);
    }
}
