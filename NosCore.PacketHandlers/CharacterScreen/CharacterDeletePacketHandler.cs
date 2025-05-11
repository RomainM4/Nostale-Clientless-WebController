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

using NosCore.Dao.Interfaces;
using NosCore.Data.CommandPackets;
using NosCore.Data.Dto;
using NosCore.Data.Enumerations.Character;
using NosCore.GameObject;
using NosCore.GameObject.Networking.ClientSession;
using NosCore.Packets.ClientPackets.CharacterSelectionScreen;
using NosCore.Packets.Enumerations;
using NosCore.Packets.ServerPackets.UI;
using NosCore.Shared.Authentication;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NosCore.Core.Configuration;

namespace NosCore.PacketHandlers.CharacterScreen
{
    public class CharacterDeletePacketHandler(IDao<CharacterDto, long> characterDao, IDao<AccountDto, long> accountDao,
            IHasher hasher, IOptions<WorldConfiguration> configuration)
        : PacketHandler<CharacterDeletePacket>, IWorldPacketHandler
    {
        public override async Task ExecuteAsync(CharacterDeletePacket packet, ClientSession clientSession)
        {
            var account = await accountDao
                .FirstOrDefaultAsync(s => s.AccountId.Equals(clientSession.Account.AccountId)).ConfigureAwait(false);
            if (account == null)
            {
                return;
            }

            if ((account.Password!.ToLower() == hasher.Hash(packet.Password!)) || (account.Name == packet.Password))
            {
                var character = await characterDao.FirstOrDefaultAsync(s =>
                    (s.AccountId == account.AccountId) && (s.Slot == packet.Slot) && (s.ServerId == configuration.Value.ServerId)
                    && (s.State == CharacterState.Active)).ConfigureAwait(false);
                if (character == null)
                {
                    return;
                }

                character.State = CharacterState.Inactive;
                character = await characterDao.TryInsertOrUpdateAsync(character).ConfigureAwait(false);

                await clientSession.HandlePacketsAsync(new[]
                {
                    new EntryPointPacket
                    {
                        Header = "EntryPoint",
                        Name = account.Name,
                        Password = account.Password
                    }
                }).ConfigureAwait(false);
            }
            else
            {
                await clientSession.SendPacketAsync(new InfoiPacket
                {
                    Message = Game18NConstString.IncorrectPassword
                }).ConfigureAwait(false);
            }
        }
    }
}