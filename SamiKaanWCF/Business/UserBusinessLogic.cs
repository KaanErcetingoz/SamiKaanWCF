// ====================================================================
// FILE 5: Business/UserBusinessLogic.cs (FROM YOUR PROJECT KNOWLEDGE)
// ====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SamiKaanWCF.Models;
using System.Diagnostics;

namespace SamiKaanWCF.Business
{
    public class UserBusinessLogic
    {
        // In-memory storage (will be replaced with database later)
        private static readonly Dictionary<string, User> _users = new Dictionary<string, User>();
        private static readonly Dictionary<string, List<UserCurrencyBalance>> _userCurrencies = new Dictionary<string, List<UserCurrencyBalance>>();

        // Replace the existing CreateAccount method in UserBusinessLogic.cs with this one

        public UserResponse CreateAccount(CreateAccountRequest request)
        {
            Debug.WriteLine("======================================================");
            Debug.WriteLine("[SERVER LOG] ==> CreateAccount method entered.");

            try
            {
                // 1. Log the received data
                if (request == null)
                {
                    Debug.WriteLine("[SERVER LOG] ==> ERROR: The entire request object is null!");
                    return new UserResponse { IsSuccess = false, Message = "Request object was null." };
                }

                Debug.WriteLine($"[SERVER LOG] ==> Received Username: '{request.Username}'");
                Debug.WriteLine($"[SERVER LOG] ==> Received Email:    '{request.Email}'");
                Debug.WriteLine($"[SERVER LOG] ==> Received Password: '{request.Password}'");

                // 2. Validate input
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    Debug.WriteLine("[SERVER LOG] ==> FAILED VALIDATION: A required field is null or empty.");
                    return new UserResponse { IsSuccess = false, Message = "Username, email, and password are required" };
                }

                // 3. Log current state of the user list
                Debug.WriteLine($"[SERVER LOG] ==> Checking for existing user. Current user count: {_users.Count}");
                foreach (var existingUser in _users.Values)
                {
                    Debug.WriteLine($"    - Stored User: {existingUser.Username}, Email: {existingUser.Email}");
                }

                // 4. Check if user already exists
                if (_users.Values.Any(u => u.Username == request.Username || u.Email == request.Email))
                {
                    Debug.WriteLine("[SERVER LOG] ==> FAILED: Username or email already exists in the dictionary.");
                    return new UserResponse { IsSuccess = false, Message = "Username or email already exists" };
                }

                // 5. If we get here, create the new user
                Debug.WriteLine("[SERVER LOG] ==> SUCCESS: User does not exist. Creating new user now.");
                var user = new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password),
                    PLNBalance = 0,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _users[user.UserId] = user;
                _userCurrencies[user.UserId] = new List<UserCurrencyBalance>();

                Debug.WriteLine($"[SERVER LOG] ==> New user '{user.Username}' created and stored successfully.");
                Debug.WriteLine("======================================================");

                return new UserResponse
                {
                    IsSuccess = true,
                    Message = "Account created successfully",
                    User = user // Return the created user details
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SERVER LOG] ==> CATASTROPHIC ERROR: An exception occurred: {ex.Message}");
                Debug.WriteLine("======================================================");
                return new UserResponse { IsSuccess = false, Message = $"An unexpected error occurred: {ex.Message}" };
            }
        }
        public UserResponse AuthenticateUser(LoginRequest request)
        {
            try
            {
                var user = _users.Values.FirstOrDefault(u => u.Username == request.Username);

                if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                {
                    return new UserResponse
                    {
                        IsSuccess = false,
                        Message = "Invalid username or password"
                    };
                }

                return new UserResponse
                {
                    IsSuccess = true,
                    Message = "Authentication successful",
                    User = new User
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = user.Email,
                        PLNBalance = user.PLNBalance,
                        CreatedDate = user.CreatedDate,
                        IsActive = user.IsActive
                    },
                    Token = GenerateToken(user.UserId)
                };
            }
            catch (Exception ex)
            {
                return new UserResponse
                {
                    IsSuccess = false,
                    Message = $"Authentication error: {ex.Message}"
                };
            }
        }

        public TransactionResponse TopUpAccount(TopUpRequest request)
        {
            try
            {
                if (!_users.ContainsKey(request.UserId))
                {
                    return new TransactionResponse
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }

                if (request.Amount <= 0)
                {
                    return new TransactionResponse
                    {
                        IsSuccess = false,
                        Message = "Top-up amount must be greater than zero"
                    };
                }

                var user = _users[request.UserId];
                user.PLNBalance += request.Amount;

                var transaction = new Transaction
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    UserId = request.UserId,
                    Type = TransactionType.TopUp,
                    CurrencyCode = "PLN",
                    Amount = request.Amount,
                    ExchangeRate = 1,
                    PLNAmount = request.Amount,
                    TransactionDate = DateTime.Now,
                    Status = TransactionStatus.Completed,
                    Reference = request.TransferReference
                };

                return new TransactionResponse
                {
                    IsSuccess = true,
                    Message = "Account topped up successfully",
                    Transaction = transaction,
                    NewPLNBalance = user.PLNBalance
                };
            }
            catch (Exception ex)
            {
                return new TransactionResponse
                {
                    IsSuccess = false,
                    Message = $"Error topping up account: {ex.Message}"
                };
            }
        }

        public BalanceResponse GetUserBalance(string userId)
        {
            try
            {
                if (!_users.ContainsKey(userId))
                {
                    return new BalanceResponse
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }

                var user = _users[userId];
                var currencyBalances = _userCurrencies.ContainsKey(userId) ? _userCurrencies[userId].ToArray() : new UserCurrencyBalance[0];

                return new BalanceResponse
                {
                    IsSuccess = true,
                    PLNBalance = user.PLNBalance,
                    CurrencyBalances = currencyBalances
                };
            }
            catch (Exception ex)
            {
                return new BalanceResponse
                {
                    IsSuccess = false,
                    Message = $"Error retrieving balance: {ex.Message}"
                };
            }
        }

        public UserResponse GetUserProfile(string userId)
        {
            try
            {
                if (!_users.ContainsKey(userId))
                {
                    return new UserResponse
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }

                var user = _users[userId];
                return new UserResponse
                {
                    IsSuccess = true,
                    User = new User
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = user.Email,
                        PLNBalance = user.PLNBalance,
                        CreatedDate = user.CreatedDate,
                        IsActive = user.IsActive
                    }
                };
            }
            catch (Exception ex)
            {
                return new UserResponse
                {
                    IsSuccess = false,
                    Message = $"Error retrieving user profile: {ex.Message}"
                };
            }
        }

        // Helper methods
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "salt"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        private string GenerateToken(string userId)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}:{DateTime.Now.Ticks}"));
        }

        // Public method to get user (for transaction service)
        public User GetUser(string userId)
        {
            return _users.ContainsKey(userId) ? _users[userId] : null;
        }

        // Public method to update user balance (for transaction service)
        public void UpdateUserBalance(string userId, decimal newBalance)
        {
            if (_users.ContainsKey(userId))
            {
                _users[userId].PLNBalance = newBalance;
            }
        }

        // Public method to update currency balance (for transaction service)
        public void UpdateCurrencyBalance(string userId, string currencyCode, decimal amount, decimal rate)
        {
            if (!_userCurrencies.ContainsKey(userId))
            {
                _userCurrencies[userId] = new List<UserCurrencyBalance>();
            }

            var existing = _userCurrencies[userId].FirstOrDefault(c => c.CurrencyCode == currencyCode);
            if (existing != null)
            {
                // Calculate new average rate
                var totalValue = (existing.Amount * existing.AverageRate) + (amount * rate);
                var totalAmount = existing.Amount + amount;
                existing.Amount = totalAmount;
                existing.AverageRate = totalAmount != 0 ? totalValue / totalAmount : 0;
            }
            else
            {
                _userCurrencies[userId].Add(new UserCurrencyBalance
                {
                    CurrencyCode = currencyCode,
                    Amount = amount,
                    AverageRate = rate
                });
            }
        }
    }
}