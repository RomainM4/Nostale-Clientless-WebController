using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Responses.Authentication
{

    public record GameAccount
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
    }

    public record AccountsResponse
    {
        public List<GameAccount> GameAccounts { get; set; } = new List<GameAccount>();
    }
}
