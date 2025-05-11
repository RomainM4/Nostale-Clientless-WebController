using NostaleSdk.Packet.Interfaces;

namespace NostaleSdk.Packet.Received
{
    public class AtPacket : ISendPacket
    {
        public int PlayerId { get; set; }
        public int MapId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Direction { get; set; }
        public int Unused0 { get; set; } = 0;
        public int Unused1 { get; set; } = 0;
        public int Unused2 { get; set; } = 1;
        public int Unused3 { get; set; } = -1;

        public string PacketToString()
        {
            return $"at {PlayerId} {MapId} {X} {Y} {Direction} {Unused0} {Unused1} {Unused2} {Unused3}";
        }
    }
}
