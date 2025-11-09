using System;
using System.Collections.Generic;

namespace SamiKaanWCF.Models
{
    public class NBPRateResponse
    {
        public string table { get; set; }
        public string currency { get; set; }
        public string code { get; set; }
        public List<NBPRate> rates { get; set; }
    }

    public class NBPRate
    {
        public string no { get; set; }
        public DateTime effectiveDate { get; set; }
        public decimal mid { get; set; }
    }

    public class NBPTableResponse
    {
        public string table { get; set; }
        public string no { get; set; }
        public DateTime effectiveDate { get; set; }
        public List<NBPCurrencyRate> rates { get; set; }
    }

    public class NBPCurrencyRate
    {
        public string currency { get; set; }
        public string code { get; set; }
        public decimal mid { get; set; }
    }
}
