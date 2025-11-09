// ====================================================================
// FILE: Models/UserModels.cs
// PURPOSE: User management and account-related models
// ====================================================================

using System;
using System.Runtime.Serialization;

namespace SamiKaanWCF.Models
{
    [DataContract]
    public class User
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string PasswordHash { get; set; }

        [DataMember]
        public decimal PLNBalance { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public bool IsActive { get; set; }
    }

    [DataContract]
    public class CreateAccountRequest
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Password { get; set; }
    }

    [DataContract]
    public class LoginRequest
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }
    }

    [DataContract]
    public class UserResponse
    {
        [DataMember]
        public bool IsSuccess { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public User User { get; set; }

        [DataMember]
        public string Token { get; set; }
    }

    [DataContract]
    public class TopUpRequest
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public string TransferReference { get; set; }
    }

    [DataContract]
    public class BalanceResponse
    {
        [DataMember]
        public bool IsSuccess { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public decimal PLNBalance { get; set; }

        [DataMember]
        public UserCurrencyBalance[] CurrencyBalances { get; set; }
    }

    [DataContract]
    public class UserCurrencyBalance
    {
        [DataMember]
        public string CurrencyCode { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public decimal AverageRate { get; set; }

        [DataMember]
        public decimal CurrentValue { get; set; }
    }
}