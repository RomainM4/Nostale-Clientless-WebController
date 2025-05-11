using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Responses.Authentication
{
    public record VersionResponse
    {
        public string Version { get; set; }
        public string MinimumVersionForDelayedUpdate { get; set; }

    }
}
