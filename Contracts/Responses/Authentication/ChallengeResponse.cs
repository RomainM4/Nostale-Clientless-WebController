using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Responses.Authentication
{
    public record ChallengeResponse
    {
        public string? id { get; set; }
        public ulong lastUpdated { get; set; }
        public string? status { get; set; }
    }
}
