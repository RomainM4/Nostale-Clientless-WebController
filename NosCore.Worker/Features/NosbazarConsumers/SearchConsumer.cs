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
using NosCore.Packets.CustomPackets.Nosbazar;

namespace NosCore.Worker.Features.NosbazarConsumers
{
    public class SearchConsumer : IConsumer<SearchRequest>
    {
        private readonly IClientControllerService _client;
        private readonly INosDataService _dataService;

        public SearchConsumer(IClientControllerService client, INosDataService dataService)
        {
            _client = client;
            _dataService = dataService;
        }

        public async Task Consume(ConsumeContext<SearchRequest> context)
        {
            if (!_client.IsInWorld())
            {
                await context.RespondAsync(new ClientConnectionExceptionResponse { Error = "Client not logged"});
            }

            var Packets = ConvertRequestToPackets(context.Message);

            var Pipeline = new ResiliencePipelineBuilder<List<NoscoreObservedPacket>>()
                 .AddTimeout(new TimeoutStrategyOptions
                 {
                     Timeout = TimeSpan.FromMilliseconds(3000),
                     OnTimeout = async args =>
                     {
                         await context.RespondAsync(new TimeoutExceptionResponse
                         {
                             Error = "Timeout : Consumers.Nosbazar.Search"
                         });
                     }
                 }).Build();


            var ObservedPackets = await Pipeline.ExecuteAsync(async token => await _client.SendPackets(Packets, token));

            if (ObservedPackets.Where(x => x.ObservedPacketName == "rc_blist").Count() <= 0)
            {
                await context.RespondAsync(new ExceptionResponse
                {
                    Error = "Packet sent but no packet received"
                });

                return;
            }

            var response = ConvertPacketsToResponse(ObservedPackets);

            await context.RespondAsync(response);

            throw new NotImplementedException();
        }

        private SearchResponse ConvertPacketsToResponse(List<NoscoreObservedPacket> observedPackets)
        {
            var RcBlistPacket = observedPackets.Where(x => x.ObservedPacketName == "rc_blist").First();

            return new SearchResponse {RcBlistObservedPacket = RcBlistPacket };
        }

        private List<NoscoreObservablePacket> ConvertRequestToPackets(SearchRequest request)
        {
            var Packets = new List<NoscoreObservablePacket>();

            var PacketBuilder = new StringBuilder();

            PacketBuilder.Append($"c_blist {(int)request.PageSearch} {(int)request.MainType} {(int)request.ClassType} {(int)request.LevelType} {(int)request.RarityType} {(int)request.UpgradeType} {(int)request.SortType} 0");
            
            //if(request.Search != string.Empty)
            //{
            //    if (!_dataService.IsInitialized())
            //    {
            //        _dataService.GenerateData(NosDataService.ITEM_DATA_PATH, NosDataService.ITEM_CODE_PATH_FR);
            //    }

            //    var ids = _dataService.GetItemIdsFromRequest(request.Search);

            //    PacketBuilder.Append(" 1");

            //    foreach (var id in ids)
            //        PacketBuilder.Append($" {id}");

            //}
            //else
            PacketBuilder.Append(" 0");


            var start_nosbazar_packet = new NoscoreObservablePacket
            {
                Value = "n_run 60 0 2 9986",
                ObservedPackets = new List<string>(),
                WaitTimeMs = 250,
                ObservationTimeoutMs = 1000
            };

            var request_blist_packet = new NoscoreObservablePacket
            {
                Value = PacketBuilder.ToString(),
                ObservedPackets = new List<string>(),
                WaitTimeMs = 250,
                ObservationTimeoutMs = 3000
            };


            request_blist_packet.ObservedPackets.Add("rc_blist");

            Packets.Add(start_nosbazar_packet);
            Packets.Add(request_blist_packet);

            return Packets;
        }
    }
}
