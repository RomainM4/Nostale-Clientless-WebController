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

using NosCore.GameObject.ComponentEntities.Interfaces;
using NosCore.Packets.Enumerations;
using NosCore.Packets.ServerPackets.Chats;
using NosCore.Packets.ServerPackets.Visibility;

namespace NosCore.GameObject.ComponentEntities.Extensions
{
    public static class VisualEntityExtension
    {
        //Pet Monster in packet
        public static InPacket GenerateIn(this INonPlayableEntity visualEntity)
        {
            return new InPacket
            {
                VisualType = visualEntity.VisualType,
                Name = visualEntity is INamedEntity namedEntity ? namedEntity.Name : null,
                VisualId = visualEntity.VisualId,
                VNum = visualEntity.VNum == 0 ? string.Empty : visualEntity.VNum.ToString(),
                PositionX = visualEntity.PositionX,
                PositionY = visualEntity.PositionY,
                Direction = visualEntity.Direction,
                InNonPlayerSubPacket = new InNonPlayerSubPacket
                {
                    Dialog = visualEntity is MapNpc npc ? npc.Dialog ?? 0 : (short)0,
                    InAliveSubPacket = new InAliveSubPacket
                    {
                        Mp = (int)(visualEntity.Mp / (float)(visualEntity.NpcMonster?.MaxMp ?? 1) * 100),
                        Hp = (int)(visualEntity.Hp / (float)(visualEntity.NpcMonster?.MaxHp ?? 1) * 100)
                    },
                    IsSitting = visualEntity.IsSitting,
                    SpawnEffect = SpawnEffectType.NoEffect,
                    Unknow1 = 2
                }
            };
        }

        //TODO move to its own class when correctly defined
        //Item in packet
        public static InPacket GenerateIn(this ICountableEntity visualEntity)
        {
            return new InPacket
            {
                VisualType = visualEntity.VisualType,
                VisualId = visualEntity.VisualId,
                VNum = visualEntity.VNum == 0 ? string.Empty : visualEntity.VNum.ToString(),
                PositionX = visualEntity.PositionX,
                PositionY = visualEntity.PositionY,
                InItemSubPacket = new InItemSubPacket
                {
                    Amount = visualEntity.Amount,
                    IsQuestRelative = false,
                    Owner = 0
                }
            };
        }

        public static SpeakPacket GenerateSpk(this INamedEntity visualEntity, SpeakPacket packet)
        {
            return new SpeakPacket
            {
                VisualType = visualEntity.VisualType,
                VisualId = visualEntity.VisualId,
                SpeakType = packet.SpeakType,
                EntityName = visualEntity.Name,
                Message = packet.Message
            };
        }
    }
}