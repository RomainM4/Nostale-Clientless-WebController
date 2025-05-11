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

using NosCore.Data.CommandPackets;
using NosCore.Data.Enumerations.I18N;
using NosCore.GameObject;
using NosCore.GameObject.ComponentEntities.Extensions;
using NosCore.GameObject.ComponentEntities.Interfaces;
using NosCore.GameObject.Networking.ClientSession;
using NosCore.Shared.Enumerations;
using Serilog;
using System.Threading.Tasks;
using NosCore.Networking;
using NosCore.Shared.I18N;

namespace NosCore.PacketHandlers.Command
{
    public class SizePacketHandler(ILogger logger, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
        : PacketHandler<SizePacket>, IWorldPacketHandler
    {
        public override Task ExecuteAsync(SizePacket sizePacket, ClientSession session)
        {
            IAliveEntity entity;
            switch (sizePacket.VisualType)
            {
                case VisualType.Player:
                    entity = session.Character!;
                    break;
                case VisualType.Monster:
                    entity = session.Character.MapInstance.Monsters.Find(s => s.VisualId == sizePacket.VisualId)!;
                    break;
                case VisualType.Npc:
                    entity = session.Character.MapInstance.Npcs.Find(s => s.VisualId == sizePacket.VisualId)!;
                    break;
                default:
                    logger.Error(logLanguage[LogLanguageKey.VISUALTYPE_UNKNOWN],
                        sizePacket.VisualType);
                    return Task.CompletedTask;
            }

            if (entity == null)
            {
                logger.Error(logLanguage[LogLanguageKey.VISUALENTITY_DOES_NOT_EXIST],
                    sizePacket.VisualType);
                return Task.CompletedTask;
            }
            entity.Size = sizePacket.Size;
            return session.Character.MapInstance.SendPacketAsync(entity.GenerateCharSc());
        }
    }
}