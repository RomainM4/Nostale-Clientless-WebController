
using Contracts.Requests.Authentication;
using Contracts.Responses.Authentication;
using FluentResults;
using NosCore.AuthApi.Contracts;

namespace NosCore.AuthApi.HttpClients.Interfaces
{
    public interface IAuthenticationClient
    {
        Task<Result<LoginResponse>> GetLoginToken(LoginRequest request);
        Task<Result<CodeResponse>> GetNostaleToken(CodeRequestEx request);
        Task<Result<AccountsResponse>> GetAccounts(AccountsRequest request);
    }
}
