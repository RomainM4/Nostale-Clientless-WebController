using Contracts.Responses;
using MassTransit;
using NostaleSdk.Configs.Interfaces;
using NostaleSdk.Configs;
using Polly;
using Polly.Timeout;
using Contracts.Requests;
using NosCore.Worker.Services.Client;

namespace NosCore.Worker.Features.ClientConnectionConsumers
{
    public class DisconnectConsumer : IConsumer<DisconnectClientConnectionRequest>
    {
        private readonly IClientControllerService _controller;

        public DisconnectConsumer(IClientControllerService client)
        {
            _controller = client;
        }

        public async Task Consume(ConsumeContext<DisconnectClientConnectionRequest> context)
        {
            DisconnectClientConnectionRequest? Request = context.Message;

            if (!_controller.IsInWorld())
            {
                await context.RespondAsync(new ClientConnectionExceptionResponse { Error = "Error client is not connected" });
                return;
            }

            _controller.Disconnect();

            await context.RespondAsync(new DisconnectClientConnectionResponse { });
        }
    }
}
