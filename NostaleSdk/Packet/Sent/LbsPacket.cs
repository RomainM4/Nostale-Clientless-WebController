using NostaleSdk.Packet.Interfaces;

namespace NostaleSdk.Packet.Sent
{
    public class LbsPacket : ISendPacket
    {
        public byte Type { get; set; }

        public string PacketToString()
        {
            return $"lbs {Type}";
        }
    }
}