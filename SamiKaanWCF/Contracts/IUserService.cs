// ===================================================================
// FILE: Contracts/IUserService.cs
// PURPOSE: Service contract with REST/JSON support for MAUI app
// ===================================================================

using System.ServiceModel;
using System.ServiceModel.Web;
using SamiKaanWCF.Models;

namespace SamiKaanWCF.Contracts
{
    [ServiceContract]
    public interface IUserService
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
                   UriTemplate = "/CreateAccount",
                   RequestFormat = WebMessageFormat.Json,
                   ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.Bare)]
        UserResponse CreateAccount(CreateAccountRequest request);

        [OperationContract]
        [WebInvoke(Method = "POST",
                   UriTemplate = "/AuthenticateUser",
                   RequestFormat = WebMessageFormat.Json,
                   ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.Bare)]
        UserResponse AuthenticateUser(LoginRequest request);

        [OperationContract]
        [WebInvoke(Method = "POST",
                   UriTemplate = "/TopUpAccount",
                   RequestFormat = WebMessageFormat.Json,
                   ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.Bare)]
        TransactionResponse TopUpAccount(TopUpRequest request);

        [OperationContract]
        [WebInvoke(Method = "POST",
                   UriTemplate = "/GetUserBalance",
                   RequestFormat = WebMessageFormat.Json,
                   ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.Bare)]
        BalanceResponse GetUserBalance(string userId);

        [OperationContract]
        [WebInvoke(Method = "POST",
                   UriTemplate = "/GetUserProfile",
                   RequestFormat = WebMessageFormat.Json,
                   ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.Bare)]
        UserResponse GetUserProfile(string userId);
    }
}