﻿//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// 
// Copyright (C) 2019 - NosCore
// 
// NosCore is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using DotNetty.Transport.Channels;
using Microsoft.Extensions.Options;
using NosCore.Core.Configuration;
using NosCore.Core.I18N;
using NosCore.Data.CommandPackets;
using NosCore.Data.Dto;
using NosCore.Data.Enumerations.I18N;
using NosCore.GameObject.ComponentEntities.Extensions;
using NosCore.GameObject.Services.ExchangeService;
using NosCore.GameObject.Services.MapInstanceGenerationService;
using NosCore.GameObject.Services.MinilandService;
using NosCore.Packets.Attributes;
using NosCore.Packets.ClientPackets.Infrastructure;
using NosCore.Packets.ClientPackets.UI;
using NosCore.Packets.Enumerations;
using NosCore.Packets.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NosCore.GameObject.InterChannelCommunication.Hubs.FriendHub;
using NosCore.GameObject.InterChannelCommunication.Hubs.PubSub;
using NosCore.GameObject.Services.SaveService;
using NosCore.Networking;
using NosCore.Networking.SessionRef;
using NosCore.Shared.I18N;


namespace NosCore.GameObject.Networking.ClientSession
{
    public class ClientSession : NetworkClient
    {
        private readonly Dictionary<Type, PacketHeaderAttribute> _attributeDic = new();

        private readonly IExchangeService _exchangeProvider = null!;
        private readonly IFriendHub _friendHttpClient = null!;
        private readonly SemaphoreSlim _handlingPacketLock = new(1, 1);
        private readonly bool _isWorldClient;
        private readonly ILogger _logger;

        private readonly IMapInstanceGeneratorService _mapInstanceGeneratorService = null!;
        private readonly IMinilandService _minilandProvider = null!;
        private readonly ISerializer _packetSerializer;
        private readonly IEnumerable<IPacketHandler> _packetsHandlers;
        private Character? _character;
        private int? _waitForPacketsAmount;
        private readonly ISessionRefHolder _sessionRefHolder;
    
        private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;
        private readonly ISaveService _saveService = null!;
        private readonly IGameLanguageLocalizer _gameLanguageLocalizer = null!;
        private readonly IPubSubHub _pubSubHub;

        public ClientSession(ILogger logger, IEnumerable<IPacketHandler> packetsHandlers,
            ISerializer packetSerializer, ISessionRefHolder sessionRefHolder,
            ILogLanguageLocalizer<NosCore.Networking.Resource.LogLanguageKey> networkingLogLanguage, ILogLanguageLocalizer<LogLanguageKey> logLanguage, IPubSubHub pubSubHub)
            : base(logger, networkingLogLanguage)
        {
            _logger = logger;
            _packetsHandlers = packetsHandlers.ToList();
            _packetSerializer = packetSerializer;
            _sessionRefHolder = sessionRefHolder;
            _logLanguage = logLanguage;
            _pubSubHub = pubSubHub;
            foreach (var handler in _packetsHandlers)
            {
                var type = handler.GetType().BaseType?.GenericTypeArguments[0]!;
                if (!_attributeDic.ContainsKey(type ?? throw new InvalidOperationException()))
                {
                    _attributeDic.Add(type, type.GetCustomAttribute<PacketHeaderAttribute>(true)!);
                }
            }
        }

        public ClientSession(IOptions<LoginConfiguration> configuration, ILogger logger,
            IEnumerable<IPacketHandler> packetsHandlers,
            ISerializer packetSerializer,  ISessionRefHolder sessionRefHolder,
            ILogLanguageLocalizer<NosCore.Networking.Resource.LogLanguageKey> networkingLogLanguage, ILogLanguageLocalizer<LogLanguageKey> logLanguage, IPubSubHub pubSubHub) 
            : this(logger, packetsHandlers, packetSerializer,
                sessionRefHolder, networkingLogLanguage, logLanguage, pubSubHub)
        {
        }

        public ClientSession(IOptions<WorldConfiguration> configuration,
            IExchangeService? exchangeService, ILogger logger,
            IEnumerable<IPacketHandler> packetsHandlers, IFriendHub friendHttpClient,
            ISerializer packetSerializer,
            IMinilandService? minilandProvider, IMapInstanceGeneratorService mapInstanceGeneratorService, ISessionRefHolder sessionRefHolder, 
            ISaveService saveService,
            ILogLanguageLocalizer<NosCore.Networking.Resource.LogLanguageKey> networkingLogLanguage, ILogLanguageLocalizer<LogLanguageKey> logLanguage, IGameLanguageLocalizer gameLanguageLocalizer, IPubSubHub pubSubHub)
            : this(logger, packetsHandlers, packetSerializer, sessionRefHolder, networkingLogLanguage, logLanguage, pubSubHub)
        {
            _exchangeProvider = exchangeService!;
            _minilandProvider = minilandProvider!;
            _isWorldClient = true;
            _mapInstanceGeneratorService = mapInstanceGeneratorService;
            _saveService = saveService;
            _gameLanguageLocalizer = gameLanguageLocalizer;
            _friendHttpClient = friendHttpClient;
        }

        public bool GameStarted { get; set; }

        public bool MfaValidated { get; set; }

        public int LastKeepAliveIdentity { get; set; }

        public IList<IPacket> WaitForPacketList { get; } = new List<IPacket>();

        public int LastPulse { get; set; }

        public AccountDto Account { get; set; } = null!;

        public Character Character
        {
            get
            {
                if ((_character == null) || HasSelectedCharacter)
                {
                    return _character!;
                }

                // cant access an
                _logger.Warning(_logLanguage[LogLanguageKey.CHARACTER_NOT_INIT]);
                throw new NullReferenceException();

            }

            private set => _character = value;
        }

        public void InitializeAccount(AccountDto accountDto)
        {
            Account = accountDto;
            IsAuthenticated = true;
            Broadcaster.Instance.RegisterSession(this);
        }

        public Task SetCharacterAsync(Character? character)
        {
            _character = character;
            HasSelectedCharacter = character != null;
            if (character == null)
            {
                return Task.CompletedTask;
            }

            Character.Session = this;
            return _minilandProvider.InitializeAsync(character, _mapInstanceGeneratorService);
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        public override async void ChannelRead(IChannelHandlerContext context, object message)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            if (!(message is IEnumerable<IPacket> buff))
            {
                return;
            }

            try
            {
                await HandlePacketsAsync(buff, context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(_logLanguage[LogLanguageKey.PACKET_HANDLING_ERROR], ex);
                await _pubSubHub.UnsubscribeAsync(SessionId);
                await DisconnectAsync();
            }
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        public override async void ChannelInactive(IChannelHandlerContext context)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            try
            {
                if (_character?.IsDisconnecting ?? true)
                {
                    return;
                }

                if (_character != null)
                {
                    Character.IsDisconnecting = true;
                    if (Character.Hp < 1)
                    {
                        Character.Hp = 1;
                    }

                    await Character.SendFinfoAsync(_friendHttpClient, _pubSubHub, _packetSerializer, false)
                        .ConfigureAwait(false);

                    var targetId = _exchangeProvider.GetTargetId(Character.VisualId);
                    if (targetId.HasValue)
                    {
                        var closeExchange =
                            _exchangeProvider.CloseExchange(Character.VisualId, ExchangeResultType.Failure);
                        if (Broadcaster.Instance.GetCharacter(s => s.VisualId == targetId) is Character target)
                        {
                            await target.SendPacketAsync(closeExchange).ConfigureAwait(false);
                        }
                    }

                    await Character.LeaveGroupAsync().ConfigureAwait(false);
                    await Character.MapInstance.SendPacketAsync(Character.GenerateOut()).ConfigureAwait(false);
                    await _saveService.SaveAsync(Character).ConfigureAwait(false);

                    var minilandId = await _minilandProvider.DeleteMinilandAsync(Character.CharacterId)
                        .ConfigureAwait(false);
                    if (minilandId != null)
                    {
                        _mapInstanceGeneratorService.RemoveMap((Guid)minilandId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Information(_logLanguage[LogLanguageKey.CLIENT_DISCONNECTED], ex);
            }
            finally
            {
                Broadcaster.Instance.UnregisterSession(this);
                await _pubSubHub.UnsubscribeAsync(this.SessionId);
                _logger.Information(_logLanguage[LogLanguageKey.CLIENT_DISCONNECTED]);
            }
        }

        public string GetMessageFromKey(LanguageKey languageKey)
        {
            return _gameLanguageLocalizer[languageKey, Account.Language];
        }

        public Task HandlePacketsAsync(IEnumerable<IPacket> packetConcatenated, IChannelHandlerContext? contex = null)
        {
            return Task.WhenAll(packetConcatenated.Select(async pack =>
            {
                var packet = pack;
                if (_isWorldClient)
                {
                    if (contex != null)
                    {
                        if ((LastKeepAliveIdentity != 0) && (packet.KeepAliveId != LastKeepAliveIdentity + 1))
                        {
                            _logger.Error(_logLanguage[LogLanguageKey.CORRUPTED_KEEPALIVE],
                                SessionId);
                            await DisconnectAsync().ConfigureAwait(false);
                            return;
                        }

                        if (!_waitForPacketsAmount.HasValue && (LastKeepAliveIdentity == 0))
                        {
                            SessionId = _sessionRefHolder[contex.Channel.Id.AsLongText()].SessionId;
                            _logger.Debug(_logLanguage[LogLanguageKey.CLIENT_ARRIVED],
                                SessionId);

                            _waitForPacketsAmount = 2;
                            return;
                        }

                        LastKeepAliveIdentity = packet.KeepAliveId ?? 0;

                        if (packet.KeepAliveId == null)
                        {
                            await DisconnectAsync().ConfigureAwait(false);
                        }
                    }

                    if (_waitForPacketsAmount.HasValue)
                    {
                        WaitForPacketList.Add(pack);
                        if (packet.Header != _attributeDic[typeof(DacPacket)].Identification)
                        {
                            if (WaitForPacketList.Count != _waitForPacketsAmount)
                            {
                                LastKeepAliveIdentity = packet.KeepAliveId ?? 0;
                                return;
                            }

                            packet = new EntryPointPacket
                            {
                                Header = "EntryPoint",
                                KeepAliveId = packet.KeepAliveId,
                                Name = WaitForPacketList[0].Header!,
                                Password = "thisisgfmode",
                            };
                        }

                        _waitForPacketsAmount = null;
                        WaitForPacketList.Clear();
                    }

                    var packetHeader = packet.Header;
                    if (string.IsNullOrWhiteSpace(packetHeader) && (contex != null))
                    {
                        _logger.Warning(_logLanguage[LogLanguageKey.CORRUPT_PACKET],
                            packet);
                        await DisconnectAsync().ConfigureAwait(false);
                        return;
                    }

                    var handler = _packetsHandlers.FirstOrDefault(s =>
                        s.GetType().BaseType?.GenericTypeArguments[0] == packet.GetType());
                    if (handler != null)
                    {
                        if (packet.IsValid)
                        {
                            var attr = _attributeDic[packet.GetType()];
                            if (HasSelectedCharacter && (attr.Scopes & Scope.InTrade) == 0 && Character.InExchangeOrShop)
                            {
                                _logger.Warning(
                                    _logLanguage[LogLanguageKey.PLAYER_IN_SHOP],
                                    packet.Header);
                                return;
                            }

                            var isMfa = packet is GuriPacket guri && guri.Type == GuriPacketType.TextInput && guri.Argument == 3 && guri.VisualId == 0;
                            if (!HasSelectedCharacter && (attr.Scopes & Scope.OnCharacterScreen) == 0 && !isMfa)
                            {
                                _logger.Warning(
                                    _logLanguage[LogLanguageKey.PACKET_USED_WITHOUT_CHARACTER],
                                    packet.Header);
                                return;
                            }

                            if (HasSelectedCharacter && (attr.Scopes & Scope.InGame) == 0)
                            {
                                _logger.Warning(
                                    _logLanguage[LogLanguageKey.PACKET_USED_WHILE_IN_GAME],
                                    packet.Header);
                                return;
                            }

                            //check for the correct authority
                            if (IsAuthenticated && attr is CommandPacketHeaderAttribute commandHeader &&
                                ((byte)commandHeader.Authority > (byte)Account.Authority))
                            {
                                return;
                            }

                            if (contex != null)
                            {
                                await _handlingPacketLock.WaitAsync();
                            }

                            try
                            {
                                await Task.WhenAll(handler.ExecuteAsync(packet, this), Task.Delay(200)).ConfigureAwait(false);
                            }
                            finally
                            {
                                if (contex != null)
                                {
                                    _handlingPacketLock.Release();
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.Warning(_logLanguage[LogLanguageKey.HANDLER_NOT_FOUND],
                            packet.Header);
                    }

                }
                else
                {
                    var packetHeader = packet.Header;
                    if (string.IsNullOrWhiteSpace(packetHeader))
                    {
                        await DisconnectAsync().ConfigureAwait(false);
                        return;
                    }

                    var attr = _attributeDic[packet.GetType()];
                    if ((attr.Scopes & Scope.OnLoginScreen) == 0)
                    {
                        _logger.Warning(
                            _logLanguage[LogLanguageKey.PACKET_USED_WHILE_NOT_ON_LOGIN],
                            packet.Header);
                        return;
                    }

                    var handler = _packetsHandlers.FirstOrDefault(s =>
                        s.GetType().BaseType?.GenericTypeArguments[0] == packet.GetType());
                    if (handler != null)
                    {
                        await handler.ExecuteAsync(packet, this).ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.Warning(_logLanguage[LogLanguageKey.HANDLER_NOT_FOUND],
                            packetHeader);
                    }
                }
            }));
        }
    }
}