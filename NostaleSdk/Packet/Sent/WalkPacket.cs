using NostaleSdk.Packet.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NostaleSdk.Packet.Sent
{
    public class WalkPacket : ISendPacket
    {
        public int X {  get; set; }
        public int Y { get; set; }
        public int Direction { get; set; }
        public int Unused0 { get; set; }

        public string PacketToString()
        {
            return $"walk {X} {Y} {Direction} {Unused0}";
        }
    }
}
