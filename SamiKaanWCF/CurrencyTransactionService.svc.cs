using System;
using SamiKaanWCF.Business;
using SamiKaanWCF.Contracts;
using SamiKaanWCF.Models;

namespace SamiKaanWCF.Service
{
    public class CurrencyTransactionService : ICurrencyTransactionService
    {
        private readonly CurrencyTransactionBusinessLogic _businessLogic;

        public CurrencyTransactionService()
        {
            _businessLogic = new CurrencyTransactionBusinessLogic();
        }

        public TransactionResponse BuyCurrency(BuyOrderRequest request)
        {
            try
            {
                return _businessLogic.BuyCurrency(request);
            }
            catch (Exception ex)
            {
                return new TransactionResponse
                {
                    IsSuccess = false,
                    Message = $"Service error: {ex.Message}"
                };
            }
        }

        public TransactionResponse SellCurrency(SellOrderRequest request)
        {
            try
            {
                return _businessLogic.SellCurrency(request);
            }
            catch (Exception ex)
            {
                return new TransactionResponse
                {
                    IsSuccess = false,
                    Message = $"Service error: {ex.Message}"
                };
            }
        }

        public TransactionHistory GetTransactionHistory(string userId, int pageSize, int pageNumber)
        {
            try
            {
                return _businessLogic.GetTransactionHistory(userId, pageSize, pageNumber);
            }
            catch (Exception ex)
            {
                return new TransactionHistory
                {
                    IsSuccess = false,
                    Message = $"Service error: {ex.Message}",
                    Transactions = new Transaction[0]
                };
            }
        }

        public TransactionResponse GetTransactionDetails(string transactionId)
        {
            try
            {
                return _businessLogic.GetTransactionDetails(transactionId);
            }
            catch (Exception ex)
            {
                return new TransactionResponse
                {
                    IsSuccess = false,
                    Message = $"Service error: {ex.Message}"
                };
            }
        }

        public ExchangeRateResponse CalculateBuyPrice(string currencyCode, decimal amount)
        {
            try
            {
                return _businessLogic.CalculateBuyPrice(currencyCode, amount);
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

        public ExchangeRateResponse CalculateSellPrice(string currencyCode, decimal amount)
        {
            try
            {
                return _businessLogic.CalculateSellPrice(currencyCode, amount);
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