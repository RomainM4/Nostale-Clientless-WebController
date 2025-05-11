using Contracts.Auth;
using Contracts.Requests;
using Contracts.Responses;
using Contracts.Responses.Nosbazar;
using FluentResults;
using MassTransit;
using System.Text.Json;
namespace NosCore.ClientApi.Services.Packet
{
    public class PacketObserverService
    {
        private readonly IRequestClient<PacketObserverRequest> _observableClient;

        public PacketObserverService(IRequestClient<PacketObserverRequest> loginClient)
        {
            _observableClient = loginClient;
        }

        public async Task<Result<PacketObserverResponse>> SendAsync(PacketObserverRequest request)
        {
            //TODO ROLES
            request.role = Role.Administrator;

            var response = await _observableClient.GetResponse<PacketObserverResponse, PacketExceptionResponse, TimeoutExceptionResponse>(request,
                timeout: TimeSpan.FromMinutes(5)
                );

            if (response.Is(out Response<PacketObserverResponse> ObservablePacketsResponse))
            {

                return Result.Ok(ObservablePacketsResponse.Message);
            }

            if (response.Is(out Response<PacketExceptionResponse> PacketException))
            {
                return Result.Fail<PacketObserverResponse>(new Error("ClientApi.Packet.Send", new Error("Packet Exception")));
            }

            if (response.Is(out Response<TimeoutExceptionResponse> TimeoutException))
            {
                return Result.Fail<PacketObserverResponse>(new Error("ClientApi.Packet.Send", new Error("Packet Timeout")));
            }

            throw new Exception("Error response not parsed");
        }


    }
}
