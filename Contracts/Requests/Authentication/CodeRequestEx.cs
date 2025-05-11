using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Requests.Authentication
{
    public record CodeRequest
    {
        public string AccountId { get; set; }
        public string Blackbox { get; set; }
        public string InstallationId { get; set; }
        public string Token { get; set; }
    }
    public record CodeRequestEx
    {
        public string Token{get; set;} 
        public string AccountId { get; set;} 
        public string InstallationId { get; set;} 
        public string Blackbox { get; set;} 
        public string BlackboxEncrypted{get; set;}
        public string ChromeVersion{get; set;}
        public string Magic{get; set;}
        public string Gsid { get; set;}
    }
}
