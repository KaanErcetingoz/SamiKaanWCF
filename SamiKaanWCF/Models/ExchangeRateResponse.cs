// ====================================================================
// FILE: Models/ExchangeRateResponse.cs
// PURPOSE: Response model for exchange rate operations
// ====================================================================

using System;
using System.Runtime.Serialization;

namespace SamiKaanWCF.Models
{
    [DataContract]
    public class ExchangeRateResponse
    {
        [DataMember]
        public string Currency { get; set; }

        [DataMember]
        public decimal Rate { get; set; }

        [DataMember]
        public string EffectiveDate { get; set; }

        [DataMember]
        public string TableType { get; set; }

        [DataMember]
        public bool IsSuccess { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public decimal TotalPrice { get; set; }
    }
}