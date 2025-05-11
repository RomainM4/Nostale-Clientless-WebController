using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Responses.Nosbazar
{
    public record ExceptionResponse
    {
        public string Error {  get; set; }
    }
}
