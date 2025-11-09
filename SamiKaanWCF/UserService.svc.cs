// ====================================================================
// FILE 7: UserService.svc.cs (FROM YOUR PROJECT KNOWLEDGE)
// ====================================================================

using System;
using SamiKaanWCF.Business;
using SamiKaanWCF.Contracts;
using SamiKaanWCF.Models;

namespace SamiKaanWCF.Service
{
    public class UserService : IUserService
    {
        private readonly UserBusinessLogic _businessLogic;

        public UserService()
        {
            _businessLogic = new UserBusinessLogic();
        }

        public UserResponse CreateAccount(CreateAccountRequest request)
        {
            try
            {
                return _businessLogic.CreateAccount(request);
            }
            catch (Exception ex)
            {
                return new UserResponse
                {
                    IsSuccess = false,
                    Message = $"Service error: {ex.Message}"
                };
            }
        }

        public UserResponse AuthenticateUser(LoginRequest request)
        {
            try
            {
                return _businessLogic.AuthenticateUser(request);
            }
            catch (Exception ex)
            {
                return new UserResponse
                {
                    IsSuccess = false,
                    Message = $"Service error: {ex.Message}"
                };
            }
        }

        public TransactionResponse TopUpAccount(TopUpRequest request)
        {
            try
            {
                return _businessLogic.TopUpAccount(request);
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

        public BalanceResponse GetUserBalance(string userId)
        {
            try
            {
                return _businessLogic.GetUserBalance(userId);
            }
            catch (Exception ex)
            {
                return new BalanceResponse
                {
                    IsSuccess = false,
                    Message = $"Service error: {ex.Message}"
                };
            }
        }

        public UserResponse GetUserProfile(string userId)
        {
            try
            {
                return _businessLogic.GetUserProfile(userId);
            }
            catch (Exception ex)
            {
                return new UserResponse
                {
                    IsSuccess = false,
                    Message = $"Service error: {ex.Message}"
                };
            }
        }
    }
}