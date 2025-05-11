using NostaleSdk.Configs.Interfaces;

namespace Contracts.Requests
{
    public record LoginClientConnectionRequest
    {
        public string Token { get; set; }
        public string AccountName { get; set; }
        public int CharacterSlot { get; set; }
        public ServerType ServerType { get; set; }
        public int CanalSlot { get; set; }
        public ProxyType ProxyType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public short Port { get; set; }

    }

    public record DisconnectClientConnectionRequest
    {
        public int Unused { get; set; }
    }
}
