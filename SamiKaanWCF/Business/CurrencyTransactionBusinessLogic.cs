// ====================================================================
// FILE: Business/CurrencyTransactionBusinessLogic.cs
// PURPOSE: Business logic for currency buying/selling operations
// ====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using SamiKaanWCF.Models;

namespace SamiKaanWCF.Business
{
    public class CurrencyTransactionBusinessLogic
    {
        private readonly ExchangeRateBusinessLogic _exchangeRateLogic;
        private readonly UserBusinessLogic _userLogic;

        // In-memory storage for transactions (will be replaced with database later)
        private static readonly Dictionary<string, Transaction> _transactions = new Dictionary<string, Transaction>();

        // Exchange office margins
        private const decimal BuyMargin = 0.02m; // 2% margin on buy
        private const decimal SellMargin = 0.02m; // 2% margin on sell

        public CurrencyTransactionBusinessLogic()
        {
            _exchangeRateLogic = new ExchangeRateBusinessLogic();
            _userLogic = new UserBusinessLogic();
        }

        public TransactionResponse BuyCurrency(BuyOrderRequest request)
        {
            try
            {
                // Validate user
                var user = _userLogic.GetUser(request.UserId);
                if (user == null)
                {
                    return new TransactionResponse
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }

                // Validate amount
                if (request.Amount <= 0)
                {
                    return new TransactionResponse
                    {
                        IsSuccess = false,
                        Message = "Amount must be greater than zero"
                    };
                }

                // Get current exchange rate
                var rateResponse = _exchangeRateLogic.GetCurrentExchangeRate(request.CurrencyCode);
                if (!rateResponse.IsSuccess)
                {
                    return new TransactionResponse
                    {
                        IsSuccess = false,
                        Message = $"Unable to get exchange rate: {rateResponse.ErrorMessage}"
                    };
                }

                // Calculate buy price (NBP rate + margin)
                var buyRate = rateResponse.Rate * (1 + BuyMargin);
                var plnCost = request.Amount * buyRate;

                // Check if user has enough PLN
                if (user.PLNBalance < plnCost)
                {
                    return new TransactionResponse
                    {
                        IsSuccess = false,
                        Message = $"Insufficient PLN balance. Required: {plnCost:F2} PLN, Available: {user.PLNBalance:F2} PLN"
                    };
                }

                // Create transaction
                var transaction = new Transaction
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    UserId = request.UserId,
                    Type = TransactionType.Buy,
                    CurrencyCode = request.CurrencyCode,
                    Amount = request.Amount,
                    ExchangeRate = buyRate,
                    PLNAmount = plnCost,
                    TransactionDate = DateTime.Now,
                    Status = TransactionStatus.Completed,
                    Reference = $"BUY-{request.CurrencyCode}-{DateTime.Now:yyyyMMddHHmmss}"
                };

                // Update balances
                _userLogic.UpdateUserBalance(request.UserId, user.PLNBalance - plnCost);
                _userLogic.UpdateCurrencyBalance(request.UserId, request.CurrencyCode, request.Amount, buyRate);

                // Store transaction
                _transactions[transaction.TransactionId] = transaction;

                return new TransactionResponse
                {
                    IsSuccess = true,
                    Message = "Currency purchased successfully",
                    Transaction = transaction,
                    NewPLNBalance = user.PLNBalance - plnCost,
                    NewCurrencyBalance = request.Amount
                };
            }
            catch (Exception ex)
            {
                return new TransactionResponse
                {
                    IsSuccess = false,
                    Message = $"Error processing buy order: {ex.Message}"
                };
            }
        }

        public TransactionResponse SellCurrency(SellOrderRequest request)
        {
            try
            {
                // Validate user
                var user = _userLogic.GetUser(request.UserId);
                if (user == null)
                {
                    return new TransactionResponse
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }

                // Check user's currency balance
                var balanceResponse = _userLogic.GetUserBalance(request.UserId);
                if (!balanceResponse.IsSuccess)
                {
                    return new TransactionResponse
                    {
                        IsSuccess = false,
                        Message = "Unable to retrieve user balance"
                    };
                }

                var currencyBalance = balanceResponse.CurrencyBalances?.FirstOrDefault(c => c.CurrencyCode == request.CurrencyCode);
                if (currencyBalance == null || currencyBalance.Amount < request.Amount)
                {
                    return new TransactionResponse
                    {
                        IsSuccess = false,
                        Message = $"Insufficient {request.CurrencyCode} balance"
                    };
                }

                // Get current exchange rate
                var rateResponse = _exchangeRateLogic.GetCurrentExchangeRate(request.CurrencyCode);
                if (!rateResponse.IsSuccess)
                {
                    return new TransactionResponse
                    {
                        IsSuccess = false,
                        Message = $"Unable to get exchange rate: {rateResponse.ErrorMessage}"
                    };
                }

                // Calculate sell price (NBP rate - margin)
                var sellRate = rateResponse.Rate * (1 - SellMargin);
                var plnAmount = request.Amount * sellRate;

                // Create transaction
                var transaction = new Transaction
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    UserId = request.UserId,
                    Type = TransactionType.Sell,
                    CurrencyCode = request.CurrencyCode,
                    Amount = request.Amount,
                    ExchangeRate = sellRate,
                    PLNAmount = plnAmount,
                    TransactionDate = DateTime.Now,
                    Status = TransactionStatus.Completed,
                    Reference = $"SELL-{request.CurrencyCode}-{DateTime.Now:yyyyMMddHHmmss}"
                };

                // Update balances
                _userLogic.UpdateUserBalance(request.UserId, user.PLNBalance + plnAmount);
                _userLogic.UpdateCurrencyBalance(request.UserId, request.CurrencyCode, -request.Amount, sellRate);

                // Store transaction
                _transactions[transaction.TransactionId] = transaction;

                return new TransactionResponse
                {
                    IsSuccess = true,
                    Message = "Currency sold successfully",
                    Transaction = transaction,
                    NewPLNBalance = user.PLNBalance + plnAmount,
                    NewCurrencyBalance = currencyBalance.Amount - request.Amount
                };
            }
            catch (Exception ex)
            {
                return new TransactionResponse
                {
                    IsSuccess = false,
                    Message = $"Error processing sell order: {ex.Message}"
                };
            }
        }

        public TransactionHistory GetTransactionHistory(string userId, int pageSize, int pageNumber)
        {
            try
            {
                var userTransactions = _transactions.Values
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToList();

                var totalCount = userTransactions.Count;
                var pagedTransactions = userTransactions
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToArray();

                return new TransactionHistory
                {
                    IsSuccess = true,
                    Transactions = pagedTransactions,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                return new TransactionHistory
                {
                    IsSuccess = false,
                    Message = $"Error retrieving transaction history: {ex.Message}",
                    Transactions = new Transaction[0]
                };
            }
        }

        public TransactionResponse GetTransactionDetails(string transactionId)
        {
            try
            {
                if (!_transactions.ContainsKey(transactionId))
                {
                    return new TransactionResponse
                    {
                        IsSuccess = false,
                        Message = "Transaction not found"
                    };
                }

                return new TransactionResponse
                {
                    IsSuccess = true,
                    Transaction = _transactions[transactionId]
                };
            }
            catch (Exception ex)
            {
                return new TransactionResponse
                {
                    IsSuccess = false,
                    Message = $"Error retrieving transaction: {ex.Message}"
                };
            }
        }

        public ExchangeRateResponse CalculateBuyPrice(string currencyCode, decimal amount)
        {
            try
            {
                var rateResponse = _exchangeRateLogic.GetCurrentExchangeRate(currencyCode);
                if (!rateResponse.IsSuccess)
                {
                    return rateResponse;
                }

                var buyRate = rateResponse.Rate * (1 + BuyMargin);
                var totalCost = amount * buyRate;

                return new ExchangeRateResponse
                {
                    Currency = currencyCode,
                    Rate = buyRate,
                    EffectiveDate = rateResponse.EffectiveDate,
                    TableType = rateResponse.TableType,
                    IsSuccess = true,
                    TotalPrice = totalCost,
                    ErrorMessage = $"Total cost: {totalCost:F2} PLN for {amount} {currencyCode}"
                };
            }
            catch (Exception ex)
            {
                return new ExchangeRateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error calculating buy price: {ex.Message}"
                };
            }
        }

        public ExchangeRateResponse CalculateSellPrice(string currencyCode, decimal amount)
        {
            try
            {
                var rateResponse = _exchangeRateLogic.GetCurrentExchangeRate(currencyCode);
                if (!rateResponse.IsSuccess)
                {
                    return rateResponse;
                }

                var sellRate = rateResponse.Rate * (1 - SellMargin);
                var totalValue = amount * sellRate;

                return new ExchangeRateResponse
                {
                    Currency = currencyCode,
                    Rate = sellRate,
                    EffectiveDate = rateResponse.EffectiveDate,
                    TableType = rateResponse.TableType,
                    IsSuccess = true,
                    TotalPrice = totalValue,
                    ErrorMessage = $"Total value: {totalValue:F2} PLN for {amount} {currencyCode}"
                };
            }
            catch (Exception ex)
            {
                return new ExchangeRateResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error calculating sell price: {ex.Message}"
                };
            }
        }
    }
}