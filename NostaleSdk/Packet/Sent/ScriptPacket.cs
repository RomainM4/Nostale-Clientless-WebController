using NostaleSdk.Packet.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NostaleSdk.Packet.Sent
{
    public class ScriptPacket : ISendPacket
    {
        public int Unused0 { get; set; }
        public int Value { get; set; }
        public string PacketToString()
        {
            return $"script {Unused0} {Value}";
        }
    }
}
