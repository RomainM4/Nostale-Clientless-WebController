using NostaleSdk.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Responses
{
    public record PacketObserverResponse
    {
        public List<NoscoreObservedPacket> Packets {  get; set; } = new List<NoscoreObservedPacket>();
    }
}
