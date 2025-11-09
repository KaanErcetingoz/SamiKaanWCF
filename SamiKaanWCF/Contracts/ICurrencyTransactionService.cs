// ====================================================================
// FILE: Contracts/ICurrencyTransactionService.cs
// PURPOSE: Service contract for currency transaction operations
// ====================================================================

using System.ServiceModel;
using SamiKaanWCF.Models;

namespace SamiKaanWCF.Contracts
{
    [ServiceContract]
    public interface ICurrencyTransactionService
    {
        [OperationContract]
        TransactionResponse BuyCurrency(BuyOrderRequest request);

        [OperationContract]
        TransactionResponse SellCurrency(SellOrderRequest request);

        [OperationContract]
        TransactionHistory GetTransactionHistory(string userId, int pageSize, int pageNumber);

        [OperationContract]
        TransactionResponse GetTransactionDetails(string transactionId);

        [OperationContract]
        ExchangeRateResponse CalculateBuyPrice(string currencyCode, decimal amount);

        [OperationContract]
        ExchangeRateResponse CalculateSellPrice(string currencyCode, decimal amount);
    }
}