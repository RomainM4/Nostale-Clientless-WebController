using Contracts.Requests;
using Contracts.Responses;
using Contracts.Responses.Nosbazar;
using FluentResults;
using MassTransit;
using NostaleSdk.Nosbazar;

namespace NosCore.ClientApi.Services.Authentication
{
    public class AuthenticationService
    {
        private readonly IRequestClient<LoginClientConnectionRequest> _loginClient;
        private readonly IRequestClient<DisconnectClientConnectionRequest> _disconnectClient;

        public AuthenticationService(IRequestClient<LoginClientConnectionRequest> loginClient, IRequestClient<DisconnectClientConnectionRequest> disconnectClient)
        {
            _loginClient = loginClient;
            _disconnectClient = disconnectClient;
        }

        public async Task<Result<LoginClientConnectionResponse>> ConnectToServerAsync(LoginClientConnectionRequest request)
        {
            var response = await _loginClient.GetResponse<LoginClientConnectionResponse, ClientConnectionExceptionResponse, TimeoutExceptionResponse>(request,
                timeout: TimeSpan.FromMinutes(5)
                );

            if (response.Is(out Response<LoginClientConnectionResponse> LoginResponse))
            {
                return Result.Ok(LoginResponse.Message);
            }

            if (response.Is(out Response<ClientConnectionExceptionResponse> clientConnectionException))
            {
                return Result.Fail(new Error("ClientApi.Authentication.Login", new Error(clientConnectionException.Message.Error)));
            }

            if (response.Is(out Response<TimeoutExceptionResponse> LoginErrorResponse))
            {
                return Result.Fail(new Error("ClientApi.Authentication.Login", new Error(LoginErrorResponse.Message.Error)));
            }

            throw new Exception("Error response not parsed");
        }

        public async Task<bool> DisconnectAsync()
        {
            var request = new DisconnectClientConnectionRequest { Unused = 0 };

            var response = await _disconnectClient.GetResponse<DisconnectClientConnectionResponse, ClientConnectionExceptionResponse, TimeoutExceptionResponse>(request,
                timeout: TimeSpan.FromMinutes(5)
                );

            if (response.Is(out Response<DisconnectClientConnectionResponse> disconnectResponse))
            {
                return true;
            }

            if (response.Is(out Response<ClientConnectionExceptionResponse> clientConnectionException))
            {
                return false;
            }

            if (response.Is(out Response<TimeoutExceptionResponse> timeoutResponse))
            {
                return false;
            }

            throw new Exception("Error response not parsed");
        }
    }
}
