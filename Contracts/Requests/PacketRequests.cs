using Contracts.Auth;
using NostaleSdk.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Requests
{
    public record PacketObserverRequest
    {
        public Role role { get; set; } = Role.User;
        public List<NoscoreObservablePacket> Packets { get; set; } = new List<NoscoreObservablePacket>();
    }
}
