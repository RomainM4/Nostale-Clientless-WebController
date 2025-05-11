using Contracts.Responses;
using MassTransit;
using NostaleSdk.Configs.Interfaces;
using NostaleSdk.Configs;
using Polly;
using Polly.Timeout;
using Contracts.Requests;
using NostaleSdk.Packet;
using NosCore.Worker.Services.Client;

namespace NosCore.Worker.Features.ClientConnectionConsumers
{
    public class ObserverConsumer : IConsumer<PacketObserverRequest>
    {
        private const int USER_TIMEOUT = 10000;

        private readonly IClientControllerService _controller;

        public ObserverConsumer(IClientControllerService client)
        {
            _controller = client;
        }

        public async Task Consume(ConsumeContext<PacketObserverRequest> context)
        {

            PacketObserverRequest? Request = context.Message;
            List<NoscoreObservedPacket> ObservedPackets = new List<NoscoreObservedPacket>();

            DateTime DateTimeStart = DateTime.UtcNow;

            switch (Request.role)
            {
                case Contracts.Auth.Role.Administrator:

                    ObservedPackets = await _controller.SendPackets(Request.Packets);

                    break;
                case Contracts.Auth.Role.User:

                    var Pipeline = new ResiliencePipelineBuilder<List<NoscoreObservedPacket>>()
                    .AddTimeout(new TimeoutStrategyOptions
                    {
                        Timeout = TimeSpan.FromMilliseconds(USER_TIMEOUT),
                        OnTimeout = async args =>
                        {
                            await context.RespondAsync(new TimeoutExceptionResponse
                            {
                                Error = "Timeout during send packet"
                            });
                        }
                    }).Build();

                    ObservedPackets = await Pipeline.ExecuteAsync(async token => await _controller.SendPackets(Request.Packets, token));

                    break;
                case Contracts.Auth.Role.Premium:
                    throw new NotImplementedException();
                    break;
                default:
                    break;
            }


            await context.RespondAsync(new PacketObserverResponse
            {
                Packets = ObservedPackets
            });
        }
    }
}
