using NostaleSdk.Packet.Interfaces;

namespace NostaleSdk.Packet.Sent
{
    public class SelectPacket : ISendPacket
    {
        public byte Slot { get; set; }

        public string PacketToString()
        {
            return $"select {Slot}";
        }
    }
}

