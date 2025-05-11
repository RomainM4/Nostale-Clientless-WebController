using FluentResults;
using Contracts.Responses.Authentication;

namespace NosCore.AuthApi.HttpClients.Interfaces
{
    public interface IVersionClient
    {
        Task<Result<VersionResponse>> GetVersion();
    }
}
