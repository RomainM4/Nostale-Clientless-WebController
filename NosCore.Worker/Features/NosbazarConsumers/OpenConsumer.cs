using MassTransit;
using NostaleSdk.Nosbazar;
using Contracts.Requests.Nosbazar;
using Contracts.Responses.Nosbazar;
using NosCore.Worker.Services.Client;
using NosCore.Worker.Services.NosData;
using Contracts.Responses;
using NostaleSdk.Packet;
using System.Text;
using Polly.Timeout;
using Polly;
using NosCore.NostaleSDK.Item.Rune;
using NosCore.NostaleSDK.Item.Stuff;
using NostaleSdk.Character;
using NostaleSdk.Item;
using System;
using NosCore.NostaleSDK;

namespace NosCore.Worker.Features.NosbazarConsumers
{
    public class OpenConsumer : IConsumer<OpenRequest>
    {
        private readonly IClientControllerService _client;
        private const int _TIMEOUT = 20000;

        public OpenConsumer(IClientControllerService client)
        {
            _client = client;
        }

        public async Task Consume(ConsumeContext<OpenRequest> context)
        {
            if (!_client.IsInWorld())
            {
                await context.RespondAsync(new ClientConnectionExceptionResponse { Error = "Client not logged" });
            }

            var Packets = new List<NoscoreObservablePacket>();

            var Pipeline = new ResiliencePipelineBuilder<List<NoscoreObservedPacket>>()
                 .AddTimeout(new TimeoutStrategyOptions
                 {
                     Timeout = TimeSpan.FromMilliseconds(_TIMEOUT),
                     OnTimeout = async args =>
                     {
                         await context.RespondAsync(new TimeoutExceptionResponse
                         {
                             Error = "Timeout during login"
                         });
                     }
                 }).Build();

            Packets.AddRange(PacketGenerator.ConvertPacketsToObservablePackets(
                    PacketGenerator.TeleportToNosville(), 450
                    ));

            Packets.AddRange(PacketGenerator.ConvertPacketsToObservablePackets(
                    PacketGenerator.GoToMarcketPlaceFromNosvilleTpPoint(), 750
                    ));

            Packets.AddRange(PacketGenerator.ConvertPacketsToObservablePackets(
                     PacketGenerator.GoToNosbazarNpc(), 750
                     ));

            var ObservedPackets = await Pipeline.ExecuteAsync(async token => await _client.SendPackets(Packets, token));

            await context.RespondAsync(new OpenResponse { });
        }

    }
}
