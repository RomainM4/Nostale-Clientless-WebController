using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NostaleSdk.Packet
{
    public record NoscoreObservablePacket
    {
        public string Value { get; set; }                   = string.Empty;
        public int WaitTimeMs { get; set; }                 = 10;
        public int ObservationTimeoutMs { get; set; }       = 100;
        public List<string> ObservedPackets { get; set; }   = new List<string>();
    }

    public record NoscoreObservedPacket
    {
        public string ObservedPacketName { get; set; }      = string.Empty;
        public string SentPacket { get; set; }              = string.Empty;
        public string ReceivePacket { get; set; }           = string.Empty;
        public bool IsObserved { get; set; }                = false;

    }

}
