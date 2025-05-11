using NostaleSdk.Nosbazar;
using NostaleSdk.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Responses.Nosbazar
{
    public record SearchResponse
    {
        public NoscoreObservedPacket RcBlistObservedPacket { get; set; }
    }

    public record NosbazarSearchNotFoundResponse
    {
        List<int> ids { get; set; }
    }
}
