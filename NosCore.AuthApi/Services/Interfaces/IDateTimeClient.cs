using FluentResults;

namespace NosCore.AuthApi.HttpClients.Interfaces
{
    public interface IDateTimeClient
    {
        Task<Result<string>> GetDateTime();
    }
}
