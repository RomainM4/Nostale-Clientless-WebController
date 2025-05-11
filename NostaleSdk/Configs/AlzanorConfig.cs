using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NostaleSdk.Configs.Interfaces;

namespace NostaleSdk.Configs
{
    public class AlzanorConfig : IConnectionConfig
    {
        private static readonly List<short> _CanalsPort = new List<short>([4010, 4011, 4012, 4013, 4014, 4015, 4016]);

        private string _GameForgeLoginIp { get; set; }
        private short _GameForgeLoginPort { get; set; }
        private string _WorldIp { get; set; } = "79.110.84.77";
        private int _CanalSlot { get; set; }

        public ProxyType ProxyType { get ; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public short Port { get; set; }

        public AlzanorConfig(ProxyType proxyType, string username, string password, string host, short port,
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
