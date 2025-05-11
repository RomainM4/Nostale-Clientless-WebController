using Contracts.Requests.Nosbazar;
using Contracts.Responses;
using Contracts.Responses.Nosbazar;
using FluentResults;
using MassTransit;
using NosCore.Packets.CustomPackets.Nosbazar;
using NostaleSdk.Nosbazar;

namespace NosCore.ClientApi.Services.Nosbazar
{
    public class NosbazarService
    {
        private readonly IRequestClient<SearchRequest> _clientSearch;
        private readonly IRequestClient<OpenRequest> _clientOpen;

        public NosbazarService(IRequestClient<SearchRequest> clientSearch, IRequestClient<OpenRequest> clientOpen)
        {
            _clientSearch = clientSearch;
            _clientOpen = clientOpen;
        }

        public async Task<Result<SearchResponse>> SearchAsync(SearchRequest request)
        {
            var response = await _clientSearch.GetResponse<SearchResponse, NosbazarSearchNotFoundResponse>(
            request
            );

            if (response.Is(out Response<SearchResponse> searchResponse))
            {
                return Result.Ok(searchResponse.Message);
            }

            if (response.Is(out Response<NosbazarSearchNotFoundResponse> searchResponseNotFound))
            {
                return Result.Fail(new Error("Timeout error"));
            }

            return Result.Ok();
        }

        public async Task<Result<OpenResponse>> OpenAsync(OpenRequest request)
        {
            var response = await _clientOpen.GetResponse<OpenResponse, TimeoutExceptionResponse>(
            request
            );

            if (response.Is(out Response<OpenResponse> openResponse))
            {
                return Result.Ok(openResponse.Message);

            }

            if (response.Is(out Response<TimeoutExceptionResponse> timeoutResponse))
            {
                return Result.Fail(new Error("Timeout error"));

            }

            return Result.Ok();
        }
    }
}
