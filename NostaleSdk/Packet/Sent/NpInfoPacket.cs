using NostaleSdk.Packet.Interfaces;

namespace NostaleSdk.Packet.Sent
{
    public class NpInfoPacket : ISendPacket
    {
        public byte Page { get; set; }

        public string PacketToString()
        {
            return $"npinfo {Page}";
        }
    }
}