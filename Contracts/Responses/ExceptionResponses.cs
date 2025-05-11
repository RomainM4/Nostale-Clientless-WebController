using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Responses
{
    public record TimeoutExceptionResponse
    {
        public string Error { get; set; }
    }

    public record ClientConnectionExceptionResponse
    {
        public string Error { get; set; }
    }

    public record PacketExceptionResponse
    {
        public string Error { get; set; }
    }

}
