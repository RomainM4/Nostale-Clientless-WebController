using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Requests.Authentication
{
    public record ChallengeRequest
    {
        public string Id {  get; set; }
        public string Locale { get; set; }
        public ulong LastUpdate { get; set; }

        public int Answer {  get; set; }

    }
}
