using NostaleSdk.Packet.Interfaces;

namespace NostaleSdk.Packet.Sent
{
    public class CClosePacket : ISendPacket
    {
        public byte Type { get; set; }

        public string PacketToString()
        {
            return $"c_close {Type}";
        }
    }
}

