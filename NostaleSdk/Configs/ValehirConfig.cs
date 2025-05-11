using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NostaleSdk.Configs.Interfaces;

namespace NostaleSdk.Configs
{
    public class ValehirConfig : IConnectionConfig
    {
        private static readonly List<short> _CanalsPort = new List<short>([4003, 4004, 4006, 4007, 4008, 4009, 4010]);

        private string _GameForgeLoginIp { get; set; }
        private short _GameForgeLoginPort { get; set; }
        private string _WorldIp { get; set; } = "79.110.84.25";
        private int _CanalSlot { get; set; }
        public ProxyType ProxyType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public short Port { get; set; }

        public ValehirConfig(ProxyType proxyType, string username, string password, string host, short port,
            int canalSlot, string gameForgeLoginIp = "79.110.84.75", short gameForgeLoginPort = 4002)
        {
            ProxyType = proxyType;
            Username = username;
            Password = password;
            Host = host;
            Port = port;

            _CanalSlot = canalSlot;
            _GameForgeLoginIp = gameForgeLoginIp;
            _GameForgeLoginPort = gameForgeLoginPort;
        }

        public string GetGameForgeLoginIp()
        {
            return _GameForgeLoginIp;
        }

        public short GetGameForgeLoginPort()
        {
            return _GameForgeLoginPort;
        }

        public short GetWolrdPort()
        {
            return _CanalsPort[_CanalSlot];
        }

        public string GetWorldIp()
        {
            return _WorldIp;
        }

    }
}
