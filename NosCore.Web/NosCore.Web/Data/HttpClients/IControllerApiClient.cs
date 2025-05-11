using Contracts.Responses.Nosbazar;
using FluentResults;
using NostaleSdk.Nosbazar;

namespace NosCore.Web.Data.HttpClients
{
    public interface IControllerApiClient
    {
        public Task<Result> OpenNosbazarAsync();

        public Task<IEnumerable<Auction>> SearchNosbazarAsync();
    }
}
