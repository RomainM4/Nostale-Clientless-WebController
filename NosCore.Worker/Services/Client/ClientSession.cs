using NostaleSdk.Configs.Interfaces;
using NostaleSdk.Configs;

using NostaleSdk.Cryptography;
using NostaleSdk.Cryptography.Interfaces;
using NosCore.Worker.Proxy;
using System.Net.Sockets;
using System.Text;
using NosCore.NostaleSDK;
using SuperSimpleTcp;
using System.Data;
using NostaleSdk.Packet.Interfaces;

namespace NosCore.Worker.Services.Client
{
    //TODO USE POLLY
    public class ClientSession
    {
        public string Token { get; set; }
        public string AccountName { get; set; }
        public int CharacterSlot { get; set; }
        public IConnectionConfig Config { get; set; }
        public bool IsInWorld { get; set; }
        public bool IsConnectionFailed { get; set; } = false;

        private event EventHandler<ClientEventArgs>? _eventPacketRecv;

        private SimpleTcpClient? _clientWorld;
        private SimpleTcpClient? _clientLogin;

        private readonly Random _random = new Random();
        private readonly IPacketCryptography _packetCryptography = new PacketCryptography();

        private int _encryptionKey;
        private int _encryptionValue;

        #region Constructors
        public ClientSession()
        {
            AccountName     = string.Empty;
            Token           = string.Empty;
            Config          = new CosmosConfig();

            _encryptionKey = 0;
            _encryptionValue = _random.Next(1000, 9999);

            _packetCryptography = new PacketCryptography();
        }

        #endregion

        #region Public Methods
        public async Task<bool> StartSession()
        {
            var ServerResult = await InitializeServerConnection();

            if (!ServerResult)
                return false;

            await InitializeWorldConnection();

            return true;
        }

        public async Task SendPacket(string packet, int waitMicroSec = 0, bool isSessionPacket = false, CancellationToken ct = default)
        {
            byte[] packetsData;

            if (!IsInWorld)
            {
                packetsData = _packetCryptography.EncryptLoginPacket(packet);
                await _clientLogin.SendAsync(packetsData, ct);
            }
            else
            {
                packetsData = _packetCryptography.EncryptWorldPacket($"{_encryptionValue++} {packet}", isSessionPacket);
                await _clientWorld.SendAsync(packetsData, ct);
            }

            if (waitMicroSec != 0)
                await Task.Delay(waitMicroSec, ct);
        }

        public void Disconnect()
        {
            _clientWorld.Disconnect();
        }

        #endregion

        #region Private Methods
        private void OnPacketReceived(string packet)
        {
            _eventPacketRecv?.Invoke(this, new ClientEventArgs(packet));
        }

        private async Task<bool> InitializeServerConnection()
        {
            if (Token.Length == 0) return false;

            if (AccountName.Length == 0) return false;

            _clientLogin = new SimpleTcpClient(Config.GetGameForgeLoginIp(), Config.GetGameForgeLoginPort());

            _clientLogin.Events.Connected += ServerConnected;
            _clientLogin.Events.Disconnected += ServerDisconnected;
            _clientLogin.Events.DataReceived += ServerDataReceived;
            _clientLogin.Events.DataSent += ServerDataSent;
            _clientLogin.Keepalive.EnableTcpKeepAlives = true;
            _clientLogin.Settings.MutuallyAuthenticate = false;
            _clientLogin.Settings.AcceptInvalidCertificates = true;
            _clientLogin.Settings.ConnectTimeoutMs = 5000;
            _clientLogin.Settings.NoDelay = true;

            if (Config.ProxyType == ProxyType.Socks5)
            {
                _clientLogin.Settings.UseProxy = true;
                _clientLogin.ProxySettings.Host = Config.Host;
                _clientLogin.ProxySettings.Port = Config.Port;
                _clientLogin.ProxySettings.Username = Config.Username;
                _clientLogin.ProxySettings.Password = Config.Password;

            }

            _clientLogin.Connect();

            await SendLoginCredentials();

            do { await Task.Delay(1000); } while (_encryptionKey == 0 && IsConnectionFailed == false);

            _clientLogin.Disconnect();

            if (IsConnectionFailed)
                return false;

            return true;
        }
        private async Task InitializeWorldConnection()
        {
            _clientWorld = new SimpleTcpClient(Config.GetWorldIp(), Config.GetWolrdPort());

            _clientWorld.Events.Connected += WorldConnected;
            _clientWorld.Events.Disconnected += WorldDisconnected;
            _clientWorld.Events.DataReceived += WorldDataReceived;
            _clientWorld.Events.DataSent += WorldDataSent;
            _clientWorld.Keepalive.EnableTcpKeepAlives = true;
            _clientWorld.Settings.MutuallyAuthenticate = false;
            _clientWorld.Settings.AcceptInvalidCertificates = true;
            _clientWorld.Settings.ConnectTimeoutMs = 5000;
            _clientWorld.Settings.NoDelay = true;

            if (Config.ProxyType == ProxyType.Socks5)
            {
                _clientWorld.Settings.UseProxy = true;
                _clientWorld.ProxySettings.Host = Config.Host;
                _clientWorld.ProxySettings.Port = Config.Port;
                _clientWorld.ProxySettings.Username = Config.Username;
                _clientWorld.ProxySettings.Password = Config.Password;

            }

            _clientWorld.Connect();


            await SendPacket($"{_encryptionKey}", waitMicroSec: 250, true);
            await SendPacket($"{AccountName} GF 2", waitMicroSec: 250);
            await SendPacket($"thisisgfmode", waitMicroSec: 250);

            await SendPacket(PacketGenerator.GenerateCClose(0), waitMicroSec: 250);
            await SendPacket(PacketGenerator.GenerateFStashEnd(), waitMicroSec: 250);
            await SendPacket(PacketGenerator.GenerateCClose(1), waitMicroSec: 250);

            await SendPacket(PacketGenerator.GenerateSelect((byte)CharacterSlot), waitMicroSec: 250);
            await SendPacket(PacketGenerator.GenerateGameStart(), waitMicroSec: 250);
            await SendPacket(PacketGenerator.GenerateLbs(0), waitMicroSec: 250);

            await SendPacket(PacketGenerator.GenerateCClose(0), waitMicroSec: 250);
            await SendPacket(PacketGenerator.GenerateBpClose(), waitMicroSec: 250);
            await SendPacket(PacketGenerator.GenerateFStashEnd(), waitMicroSec: 50);
            await SendPacket(PacketGenerator.GenerateCClose(1), waitMicroSec: 50);
            await SendPacket(PacketGenerator.GenerateCClose(0), waitMicroSec: 50);
            await SendPacket(PacketGenerator.GenerateFStashEnd(), waitMicroSec: 50);
            await SendPacket(PacketGenerator.GenerateCClose(1), waitMicroSec: 50);
            await SendPacket($"glist 0 0", waitMicroSec: 50);
            await SendPacket(PacketGenerator.GenerateNpInfo(0), waitMicroSec: 50);
        }


        private async Task SendLoginCredentials(string installationId = "NONE_CII", int regionCode = 2, string versionNostaleClientX = "0.9.3.3232", string hashNostaleClients = "C52F10ACF54D9396E0DB71A36A4D41FF")
        {
            var packet =
                    $"NoS0577" + " " +
                    $"{Token}" + "  " +
                    $"{installationId}" + " " +
                    $"{BitConverter.ToString(Encoding.UTF8.GetBytes(_random.Next(1000, 9999).ToString())).ToUpper().Replace("-", "")}" + " " +
                    $"{regionCode}{(char)0xB}" +
                    $"{versionNostaleClientX}" + " 0 " +
                    $"{hashNostaleClients}";
            await SendPacket(packet);

        }

        public void SetOnPacketReceiveHandler(EventHandler<ClientEventArgs> func)
        {
            _eventPacketRecv += func;
        }
        public void RemoveOnPacketReceiveHandler(EventHandler<ClientEventArgs> func)
        {
            _eventPacketRecv -= func;
        }

        #endregion

        #region Callbacks

        private void WorldConnected(object sender, ConnectionEventArgs e)
        {
            IsInWorld = true;
            Console.WriteLine("*** Server " + e.IpPort + " connected");
        }

        private void WorldDisconnected(object sender, ConnectionEventArgs e)
        {
            IsInWorld = false;
            Console.WriteLine("*** Server " + e.IpPort + " disconnected");
        }

        private void WorldDataReceived(object sender, DataReceivedEventArgs e)
        {
            byte[] EncryptedPacket = e.Data.Array;
            int ReceivedPacketSize = e.Data.Count;

            var list = new List<string>();

            try
            {


                list.AddRange(_packetCryptography.DecryptWorldPacket(EncryptedPacket, ReceivedPacketSize));

            }
            catch { }

            foreach (var packet in list)
            {
                OnPacketReceived(packet);
                Console.WriteLine("[" + e.IpPort + "] " + packet);
            }
        }

        private void WorldDataSent(object sender, DataSentEventArgs e)
        {
            Console.WriteLine("[" + e.IpPort + "] sent " + e.BytesSent + " bytes");
        }



        private void ServerConnected(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine("*** Server " + e.IpPort + " connected");
        }

        private void ServerDisconnected(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine("*** Server " + e.IpPort + " disconnected");
        }

        private void ServerDataReceived(object sender, DataReceivedEventArgs e)
        {
            byte[] EncryptedPacket = e.Data.Array;
            int ReceivedPacketSize = e.Data.Count;

            var list = new List<string>();

            try
            {
                list.Add(_packetCryptography.DecryptLoginPacket(EncryptedPacket, ReceivedPacketSize));
            }
            catch { }

            var Packet = list.First();

            var SP = Packet.Split(" ");

            if (SP[0] == "failc")
                IsConnectionFailed = true;

            if (SP[0] == "NsTeST")
            {
                _encryptionKey = Convert.ToInt32(SP[125]);
                _packetCryptography.SetEncryptionKey(_encryptionKey);
            }

            Console.WriteLine("[" + e.IpPort + "] " + Packet);
        }

        private void ServerDataSent(object sender, DataSentEventArgs e)
        {
            Console.WriteLine("[" + e.IpPort + "] sent " + e.BytesSent + " bytes");
        }

        #endregion

    }

    public class ClientEventArgs
    {
        public ClientEventArgs(string packet)
        {
            Packet = packet;
        }
        public string Packet { get; set; }
    }

}
