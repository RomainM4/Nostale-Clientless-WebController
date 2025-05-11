using Contracts.Responses.Nosbazar;
using NosCore.NostaleSDK;
using NosCore.NostaleSDK.Item.Rune;
using NosCore.NostaleSDK.Item.Stuff;
using NosCore.Packets.CustomPackets.Nosbazar;
using NosCore.Worker.Services.Client;
using NosCore.Worker.Services.NosData;
using NostaleSdk.Character;
using NostaleSdk.Configs;
using NostaleSdk.Item;
using NostaleSdk.Nosbazar;
using NostaleSdk.Packet;
using System.Text;

namespace NosCore.Worker;

public class Worker : BackgroundService
{



    public static List<String> ReadPacketFilters = new List<String>(["mv", "cond", "stat"]);

    private readonly ILogger<Worker> _logger;
    private readonly IClientControllerService _controller;
    private readonly INosDataService _dataService;

    public Worker(ILogger<Worker> logger, IClientControllerService controller, INosDataService dataService)
    {
        _logger = logger;
        _controller = controller;
        _dataService = dataService;
    }


    private SearchResponse ConvertPacketsToResponse(List<NoscoreObservedPacket> observedPackets)
    {
        List<Auction> NosbazarOffers = new List<Auction>();

        var RcBlistPacket = observedPackets.Where(x => x.ObservedPacketName == "rc_blist").First();//.ReceivePacket;
        //var RcBlistSplited = RcBlistPacket.Split(" ");

        //var x = RcBlistCustomPacket.RcBlistParser.Parse(RcBlistPacket);
        

        return new SearchResponse { RcBlistObservedPacket = RcBlistPacket };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var Token = "38333435386330622D363662392D346630322D396436622D616461326536343038656661";
        var AccountName = "FR_mytest1";
        var CharacterSlot = 0;

        //var config = new CosmosConfig(NostaleSdk.Configs.Interfaces.ProxyType.Socks5, "", "", "199.58.185.9", 4145, 0);
        var config = new CosmosConfig();

        if (!_dataService.IsInitialized())
        {
            _dataService.GenerateData(NosDataService.ITEM_DATA_PATH, NosDataService.ITEM_CODE_PATH_FR);
        }

        //var PacketBuilder = new StringBuilder();

        //var ids = _dataService.GetItemIdsFromRequest("bois");

        //PacketBuilder.Append("c_blist 0 0 0 0 0 0 0 0 1");

        //foreach (var id in ids)
        //    PacketBuilder.Append($" {id}");

        //await _controller.ConnectToServer(config, Token, AccountName, CharacterSlot);

        //await Task.Delay(1000);


        //await _controller.SendPackets(PacketGenerator.ConvertPacketsToObservablePackets(
        //        PacketGenerator.TeleportToNosville(), 450
        //        ));

        //await _controller.SendPackets(PacketGenerator.ConvertPacketsToObservablePackets(
        //        PacketGenerator.GoToMarcketPlaceFromNosvilleTpPoint(), 450
        //        ));

        //await _controller.SendPackets(PacketGenerator.ConvertPacketsToObservablePackets(
        //        PacketGenerator.GoToNosbazarNpc(), 1000
        //        ));

        //var packets = new List<NoscoreObservablePacket>();

        //var start_nosbazar_packet = new NoscoreObservablePacket
        //{
        //    Value = "n_run 60 0 2 9945",
        //    ObservedPackets = new List<string>(),
        //    WaitTimeMs = 250,
        //    ObservationTimeoutMs = 1000
        //};

        //var request_blist_packet = new NoscoreObservablePacket
        //{
        //    Value = PacketBuilder.ToString(),
        //    ObservedPackets = new List<string>(),
        //    WaitTimeMs = 250,
        //    ObservationTimeoutMs = 3000
        //};

        //var request_slist_packet = new NoscoreObservablePacket
        //{
        //    Value = "c_slist 0 0 0 ",
        //    ObservedPackets = new List<string>(),
        //    WaitTimeMs = 250,
        //    ObservationTimeoutMs = 3000
        //};


        //request_blist_packet.ObservedPackets.Add("rc_blist");
        //request_slist_packet.ObservedPackets.Add("rc_slist");

        //packets.Add(start_nosbazar_packet);
        //packets.Add(request_blist_packet);
        //packets.Add(request_slist_packet);

        //var ObservedPackets = await _controller.SendPackets(packets);

        //ConvertPacketsToResponse(ObservedPackets);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(100000, stoppingToken);
        }
    }
}
