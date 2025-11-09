// ====================================================================
// FILE: Models/TransactionResponse.cs
// PURPOSE: Response model for transaction operations
// ====================================================================

using System;
using System.Runtime.Serialization;

namespace SamiKaanWCF.Models
{
    [DataContract]
    public class TransactionResponse
    {
        [DataMember]
        public bool IsSuccess { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public Transaction Transaction { get; set; }

        [DataMember]
        public decimal NewPLNBalance { get; set; }

        [DataMember]
        public decimal NewCurrencyBalance { get; set; }
    }

    [DataContract]
    public class Transaction
    {
        [DataMember]
        public string TransactionId { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public TransactionType Type { get; set; }

        [DataMember]
        public string CurrencyCode { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public decimal ExchangeRate { get; set; }

        [DataMember]
        public decimal PLNAmount { get; set; }

        [DataMember]
        public DateTime TransactionDate { get; set; }

        [DataMember]
        public TransactionStatus Status { get; set; }

        [DataMember]
        public string Reference { get; set; }
    }

    [DataContract]
    public enum TransactionType
    {
        [EnumMember]
        TopUp = 0,
        [EnumMember]
        Buy = 1,
        [EnumMember]
        Sell = 2
    }

    [DataContract]
    public enum TransactionStatus
    {
        [EnumMember]
        Pending = 0,
        [EnumMember]
        Completed = 1,
        [EnumMember]
        Failed = 2,
        [EnumMember]
        Cancelled = 3
    }

    [DataContract]
    public class BuyOrderRequest
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string CurrencyCode { get; set; }

        [DataMember]
        public decimal Amount { get; set; }
    }

    [DataContract]
    public class SellOrderRequest
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string CurrencyCode { get; set; }

        [DataMember]
        public decimal Amount { get; set; }
    }

    [DataContract]
    public class TransactionHistory
    {
        [DataMember]
        public bool IsSuccess { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public Transaction[] Transactions { get; set; }

        [DataMember]
        public int TotalCount { get; set; }
    }
}