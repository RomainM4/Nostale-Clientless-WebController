using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NostaleSdk.Configs.Interfaces
{

    public enum ServerType
    {
        Alzanor,
        Cosmos,
        Dragonveil,
        Jotunheim,
        Valehir
    }

    public enum ProxyType
    {
        /// <summary>
        /// No Proxy specified.  Note this option will cause an exception to be thrown if used to create a proxy object by the factory.
        /// </summary>
        None,
        /// <summary>
        /// HTTP Proxy
        /// </summary>
        Http,
        /// <summary>
        /// SOCKS v4 Proxy
        /// </summary>
        Socks4,
        /// <summary>
        /// SOCKS v4a Proxy
        /// </summary>
        Socks4a,
        /// <summary>
        /// SOCKS v5 Proxy
        /// </summary>
        Socks5
    }

    public interface IConnectionConfig
    {
        ProxyType ProxyType { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        string Host { get; set; }
        short Port { get; set; }

        string GetGameForgeLoginIp();
        short GetGameForgeLoginPort();
        string GetWorldIp();
        short GetWolrdPort();
    }
}
