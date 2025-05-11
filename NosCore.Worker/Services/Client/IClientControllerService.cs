using NostaleSdk.Configs.Interfaces;
using NostaleSdk.Packet;

namespace NosCore.Worker.Services.Client
{
    public interface IClientControllerService
    {
        public Task<List<NoscoreObservedPacket>> ConnectToServer(IConnectionConfig connectionConfig, string token, string username, int characterSlot, CancellationToken ct = default);

        public Task<List<NoscoreObservedPacket>> SendPackets(List<NoscoreObservablePacket> packetObversables, CancellationToken ct = default);

        public void Disconnect();

        public bool IsInWorld();

        public Client GetClient();
    }
}
