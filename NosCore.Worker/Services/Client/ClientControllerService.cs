using NosCore.Packets.ClientPackets.Battle;
using NosCore.Packets.ClientPackets.Bazaar;
using NosCore.Packets.ClientPackets.Chat;
using NosCore.Packets.ClientPackets.Commands;
using NosCore.Packets.ClientPackets.Families;
using NosCore.Packets.ClientPackets.Inventory;
using NosCore.Packets.ClientPackets.Login;
using NosCore.Packets.ClientPackets.Movement;
using NosCore.Packets.ClientPackets.Npcs;
using NosCore.Packets.ClientPackets.Relations;
using NosCore.Packets.ClientPackets.Shops;
using NosCore.Packets.ClientPackets.Warehouse;
using NosCore.Packets.Interfaces;
using NosCore.Packets;
using NosCore.Packets.ServerPackets.CharacterSelectionScreen;
using NosCore.Packets.ServerPackets.Entities;
using NosCore.Packets.ServerPackets.Login;
using NosCore.Packets.ServerPackets.Relations;
using NosCore.Packets.ServerPackets.UI;
using NostaleSdk.Configs.Interfaces;
using NostaleSdk.Packet;

namespace NosCore.Worker.Services.Client
{
    public record Client
    {
        public ClistPacket CListPacket { get; set; } = new ClistPacket();
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

    }
    public class ClientControllerService : IClientControllerService
    {
        private readonly IConnectionConfig _connectionConfig;
        private ClientSession _session;
        private Client _client;

        private readonly Dictionary<string, TaskCompletionSource<bool>> _pendingTasks = new Dictionary<string, TaskCompletionSource<bool>>();
        private List<string> _observedPackets = new List<string>();

        static readonly IDeserializer Deserializer = new Deserializer(
        new[] {
                typeof(ClistPacket)
        });

        public ClientControllerService()
        {
            _session = new ClientSession();
            _client = new Client();
        }

        private void OnPacketReceive(object sender, ClientEventArgs args)
        {
            var Packet = args.Packet;
            var SP = Packet.Split(" ");

            if (Packet.StartsWith("at"))
                _client.Id = int.Parse(SP[1]);

            if (Packet.StartsWith("clist " + _session.CharacterSlot))
            {
                _client.CListPacket = (ClistPacket)Deserializer.Deserialize(Packet);
            }


            if (_pendingTasks.ContainsKey(SP[0]))
            {
                _observedPackets.Add(Packet);
                _pendingTasks[SP[0]].SetResult(true);
            }

        }

        private async Task WaitForPacket(CancellationToken ct)
        {
            foreach (var completion in _pendingTasks)
                await completion.Value.Task.WaitAsync(ct);
        }

        //Packet, WaitTime
        public async Task<List<NoscoreObservedPacket>> SendPackets(List<NoscoreObservablePacket> packetObversables, CancellationToken ct)
        {
            var ObservedPackets = new List<NoscoreObservedPacket>();

            _session.SetOnPacketReceiveHandler(OnPacketReceive);

            foreach (var packet in packetObversables)
            {
                foreach (var observedPacket in packet.ObservedPackets)
                    _pendingTasks.Add(observedPacket, new TaskCompletionSource<bool>());


                await _session.SendPacket(packet.Value, packet.WaitTimeMs);

                if (ct != default)
                {
                    try
                    {
                        foreach (var completion in _pendingTasks)
                        {
                            if(packet.ObservedPackets.Contains(completion.Key))
                                await completion.Value.Task.WaitAsync(ct);
                        }
                    }
                    catch { }
                }
                else
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(packet.ObservationTimeoutMs)))
                    {
                        try
                        {
                            foreach (var completion in _pendingTasks)
                            {
                                if (packet.ObservedPackets.Contains(completion.Key))
                                    await completion.Value.Task.WaitAsync(ct);
                            }
                        }
                        catch { }
                    }
                }

                foreach (var observedPacket in _observedPackets)
                {
                    ObservedPackets.Add(new NoscoreObservedPacket
                    {
                        ObservedPacketName = observedPacket.Split(" ")[0],
                        SentPacket = packet.Value,
                        ReceivePacket = observedPacket,
                        IsObserved = true
                    });
                }

                foreach (var observedPacket in packet.ObservedPackets)
                {
                    if (_observedPackets.Where(str => str.StartsWith(observedPacket)).Count() > 0)
                        continue;

                    ObservedPackets.Add(new NoscoreObservedPacket
                    {
                        ObservedPacketName = observedPacket,
                        SentPacket = packet.Value,
                        ReceivePacket = "[TIMEOUT]",
                        IsObserved = false
                    });

                }

                _pendingTasks.Clear();
                _observedPackets.Clear();
            }

            _session.RemoveOnPacketReceiveHandler(OnPacketReceive);

            return ObservedPackets;
        }

        public async Task<List<NoscoreObservedPacket>> ConnectToServer(IConnectionConfig connectionConfig, string token, string username, int characterSlot, CancellationToken ct = default)
        {
            //Set credentials
            _session.AccountName = username;
            _session.Token = token;
            _session.Config = connectionConfig;
            _session.CharacterSlot = characterSlot;

            if (_session.IsInWorld)
                _session.Disconnect();

            _session.SetOnPacketReceiveHandler(OnPacketReceive);

            //Set packet you want to wait for after login 
            _pendingTasks.Add("clist", new TaskCompletionSource<bool>());
            _pendingTasks.Add("at", new TaskCompletionSource<bool>());

            bool Result = await _session.StartSession();

            if (Result)
            {
                await WaitForPacket(ct);

                var Packets = new List<NoscoreObservedPacket>();

                foreach (var item in _observedPackets)
                {
                    Packets.Add(new NoscoreObservedPacket
                    {
                        IsObserved = true,
                        SentPacket = "NULL",
                        ObservedPacketName = item.Split(" ").First(),
                        ReceivePacket = item
                    });
                }

                _pendingTasks.Clear();
                _observedPackets.Clear();
                _session.RemoveOnPacketReceiveHandler(OnPacketReceive);

                return Packets;

            }

            _pendingTasks.Clear();
            _observedPackets.Clear();
            _session.RemoveOnPacketReceiveHandler(OnPacketReceive);

            return null;
        }

        public void Disconnect()
        {
            _session.Disconnect();
            _session.IsInWorld = false;
        }
        public bool IsInWorld()
        {
            return _session.IsInWorld;
        }
        public Client GetClient() { return _client; }
    }
}
