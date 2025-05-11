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

using NosCore.Data.Enumerations;
using NosCore.GameObject.ComponentEntities.Interfaces;
using NosCore.GameObject.Services.InventoryService;
using NosCore.GameObject.Services.ItemGenerationService.Item;
using NosCore.Packets.ClientPackets.Inventory;
using NosCore.Packets.ClientPackets.Npcs;
using NosCore.Packets.Enumerations;
using NosCore.Packets.ServerPackets.Chats;
using NosCore.Packets.ServerPackets.Entities;
using NosCore.Packets.ServerPackets.Groups;
using NosCore.Packets.ServerPackets.Inventory;
using NosCore.Packets.ServerPackets.Movement;
using NosCore.Packets.ServerPackets.Player;
using NosCore.Packets.ServerPackets.Shop;
using NosCore.PathFinder.Interfaces;
using NosCore.Shared.Enumerations;
using NosCore.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NodaTime;
using NosCore.Networking;


namespace NosCore.GameObject.ComponentEntities.Extensions
{
    public static class AliveEntityExtension
    {
        public static Task ChangeDirAsync(this IAliveEntity aliveEntity, byte direction)
        {
            aliveEntity.Direction = direction;
            return aliveEntity.MapInstance.SendPacketAsync(
                aliveEntity.GenerateChangeDir());
        }

        public static DirPacket GenerateChangeDir(this IAliveEntity namedEntity)
        {
            return new DirPacket
            {
                VisualType = namedEntity.VisualType,
                VisualId = namedEntity.VisualId,
                Direction = namedEntity.Direction
            };
        }

        public static RequestNpcPacket GenerateNpcReq(this IAliveEntity namedEntity, long dialog)
        {
            return new RequestNpcPacket
            {
                Type = namedEntity.VisualType,
                TargetId = namedEntity.VisualId,
                Data = dialog
            };
        }

        public static PinitSubPacket GenerateSubPinit(this INamedEntity namedEntity, int groupPosition)
        {
            return new PinitSubPacket
            {
                VisualType = namedEntity.VisualType,
                VisualId = namedEntity.VisualId,
                GroupPosition = groupPosition,
                Level = namedEntity.Level,
                Name = namedEntity.Name,
                Gender = (namedEntity as ICharacterEntity)?.Gender ?? GenderType.Male,
                Race = namedEntity.Race,
                Morph = namedEntity.Morph,
                HeroLevel = namedEntity.HeroLevel
            };
        }

        public static PidxSubPacket GenerateSubPidx(this IAliveEntity playableEntity)
        {
            return playableEntity.GenerateSubPidx(false);
        }

        public static PidxSubPacket GenerateSubPidx(this IAliveEntity playableEntity, bool isMemberOfGroup)
        {
            return new PidxSubPacket
            {
                IsGrouped = isMemberOfGroup,
                VisualId = playableEntity.VisualId
            };
        }

        public static StPacket GenerateStatInfo(this IAliveEntity aliveEntity)
        {
            return new StPacket
            {
                Type = aliveEntity.VisualType,
                VisualId = aliveEntity.VisualId,
                Level = aliveEntity.Level,
                HeroLvl = aliveEntity.HeroLevel,
                HpPercentage = (int)(aliveEntity.Hp / (float)aliveEntity.MaxHp * 100),
                MpPercentage = (int)(aliveEntity.Mp / (float)aliveEntity.MaxMp * 100),
                CurrentHp = aliveEntity.Hp,
                CurrentMp = aliveEntity.Mp,
                BuffIds = null
            };
        }

        public static Task MoveAsync(this INonPlayableEntity nonPlayableEntity, IHeuristic distanceCalculator, IClock clock)
        {
            if (!nonPlayableEntity.IsAlive)
            {
                return Task.CompletedTask;
            }

            if (!nonPlayableEntity.IsMoving || (nonPlayableEntity.Speed <= 0))
            {
                return Task.CompletedTask;
            }

            var time = (clock.GetCurrentInstant().Minus(nonPlayableEntity.LastMove)).TotalMilliseconds;

            if (!(time > RandomHelper.Instance.RandomNumber(400, 3200)))
            {
                return Task.CompletedTask;
            }

            var mapX = nonPlayableEntity.MapX;
            var mapY = nonPlayableEntity.MapY;
            if (!nonPlayableEntity.MapInstance.Map.GetFreePosition(ref mapX, ref mapY,
                (byte)RandomHelper.Instance.RandomNumber(0, 3),
                (byte)RandomHelper.Instance.RandomNumber(0, 3)))
            {
                return Task.CompletedTask;
            }

            var distance = (int)distanceCalculator.GetDistance((nonPlayableEntity.PositionX, nonPlayableEntity.PositionY), (mapX, mapY));
            var value = 1000d * distance / (2 * nonPlayableEntity.Speed);
            Observable.Timer(TimeSpan.FromMilliseconds(value))
                .Subscribe(
                    _ =>
                    {
                        nonPlayableEntity.PositionX = mapX;
                        nonPlayableEntity.PositionY = mapY;
                    });

            nonPlayableEntity.LastMove = clock.GetCurrentInstant().Plus(Duration.FromMilliseconds(value));
            return nonPlayableEntity.MapInstance.SendPacketAsync(
                nonPlayableEntity.GenerateMove(mapX, mapY));
        }

        public static Task RestAsync(this IAliveEntity aliveEntity)
        {
            aliveEntity.IsSitting = !aliveEntity.IsSitting;
            return aliveEntity.MapInstance.SendPacketAsync(
                aliveEntity.GenerateRest());
        }

        public static CondPacket GenerateCond(this IAliveEntity aliveEntity)
        {
            return new CondPacket
            {
                VisualType = aliveEntity.VisualType,
                VisualId = aliveEntity.VisualId,
                NoAttack = aliveEntity.NoAttack,
                NoMove = aliveEntity.NoMove,
                Speed = aliveEntity.Speed
            };
        }

        public static SayPacket GenerateSay(this IAliveEntity aliveEntity, string message, SayColorType type)
        {
            return new SayPacket
            {
                VisualType = aliveEntity.VisualType,
                VisualId = aliveEntity.VisualId,
                Type = type,
                Message = message
            };
        }

        public static SayItemPacket GenerateSayItem(this IAliveEntity aliveEntity, string message, InventoryItemInstance item)
        {
            var isNormalItem = item.Type != NoscorePocketType.Equipment && item.Type != NoscorePocketType.Specialist;
            return new SayItemPacket
            {
                VisualType = aliveEntity.VisualType,
                VisualId = aliveEntity.VisualId,
                OratorSlot = 17,
                Message = message,
                IconInfo = isNormalItem ? new IconInfoPacket
                {
                    IconId = item.ItemInstance!.ItemVNum
                } : null,
                EquipmentInfo = isNormalItem ? null : new EInfoPacket(),
                SlInfo = item.Type != NoscorePocketType.Specialist ? null : new SlInfoPacket()
            };
        }

        public static ShopPacket GenerateShop(this IAliveEntity visualEntity, RegionType language)
        {
            return new ShopPacket
            {
                VisualType = visualEntity.VisualType,
                VisualId = visualEntity.VisualId,
                ShopId = visualEntity.Shop?.ShopId ?? 0,
                MenuType = visualEntity.Shop?.MenuType ?? 0,
                ShopType = visualEntity.Shop?.ShopType,
                Name = visualEntity.Shop?.Name[language]
            };
        }

        public static UseItemPacket GenerateUseItem(this IAliveEntity aliveEntity, PocketType type, short slot,
            byte mode, byte parameter)
        {
            return new UseItemPacket
            {
                VisualId = aliveEntity.VisualId,
                VisualType = aliveEntity.VisualType,
                Type = type,
                Slot = slot,
                Mode = mode,
                Parameter = parameter
            };
        }

        public static PairyPacket GeneratePairy(this IAliveEntity aliveEntity, WearableInstance? fairy)
        {
            var isBuffed = false; //TODO aliveEntity.Buff.Any(b => b.Card.CardId == 131);
            return new PairyPacket
            {
                VisualType = aliveEntity.VisualType,
                VisualId = aliveEntity.VisualId,
                FairyMoveType = fairy == null ? 0 : 4,
                Element = fairy?.Item?.Element ?? 0,
                ElementRate = fairy?.ElementRate + fairy?.Item?.ElementRate ?? 0,
                Morph = fairy?.Item?.Morph ?? 0 + (isBuffed ? 5 : 0)
            };
        }

        public static CModePacket GenerateCMode(this IAliveEntity aliveEntity)
        {
            return new CModePacket
            {
                VisualType = aliveEntity.VisualType,
                VisualId = aliveEntity.VisualId,
                Morph = aliveEntity.Morph,
                MorphUpgrade = aliveEntity.MorphUpgrade,
                MorphDesign = aliveEntity.MorphDesign,
                MorphBonus = aliveEntity.MorphBonus,
                Size = aliveEntity.Size
            };
        }

        public static CharScPacket GenerateCharSc(this IAliveEntity aliveEntity)
        {
            return new CharScPacket
            {
                VisualType = aliveEntity.VisualType,
                VisualId = aliveEntity.VisualId,
                Size = aliveEntity.Size
            };
        }

        public static MovePacket GenerateMove(this IAliveEntity aliveEntity)
        {
            return aliveEntity.GenerateMove(null, null);
        }

        public static MovePacket GenerateMove(this IAliveEntity aliveEntity, short? mapX, short? mapY)
        {
            return new MovePacket
            {
                VisualEntityId = aliveEntity.VisualId,
                MapX = mapX ?? aliveEntity.PositionX,
                MapY = mapY ?? aliveEntity.PositionY,
                Speed = aliveEntity.Speed,
                VisualType = aliveEntity.VisualType
            };
        }

        public static EffectPacket GenerateEff(this IAliveEntity aliveEntity, int effectid)
        {
            return new EffectPacket
            {
                EffectType = aliveEntity.VisualType,
                VisualEntityId = aliveEntity.VisualId,
                Id = effectid
            };
        }

        public static SayPacket GenerateSay(this IAliveEntity aliveEntity, SayPacket packet)
        {
            return new SayPacket
            {
                VisualType = aliveEntity.VisualType,
                VisualId = aliveEntity.VisualId,
                Type = packet.Type,
                Message = packet.Message
            };
        }

        public static RestPacket GenerateRest(this IAliveEntity aliveEntity)
        {
            return new RestPacket
            {
                VisualType = aliveEntity.VisualType,
                VisualId = aliveEntity.VisualId,
                IsSitting = aliveEntity.IsSitting
            };
        }

        public static PflagPacket GeneratePFlag(this IAliveEntity aliveEntity)
        {
            return new PflagPacket
            {
                VisualType = aliveEntity.VisualType,
                VisualId = aliveEntity.VisualId,
                Flag = aliveEntity.Shop?.ShopId ?? 0
            };
        }

        public static void SetLevel(this INamedEntity experiencedEntity, byte level)
        {
            experiencedEntity.Level = level;
            experiencedEntity.LevelXp = 0;
            experiencedEntity.Hp = experiencedEntity.MaxHp;
            experiencedEntity.Mp = experiencedEntity.MaxMp;
        }

        public static NInvPacket GenerateNInv(this IAliveEntity aliveEntity, double percent, short typeshop)
        {
            var shopItemList = new List<NInvItemSubPacket?>();
            var list = aliveEntity.Shop!.ShopItems.Values.Where(s => s.Type == typeshop).ToList();
            for (var i = 0; i < aliveEntity.Shop.Size; i++)
            {
                var item = list.Find(s => s.Slot == i);
                if (item == null)
                {
                    shopItemList.Add(null);
                }
                else
                {
                    shopItemList.Add(new NInvItemSubPacket
                    {
                        Type = (PocketType)item.ItemInstance!.Item.Type,
                        Slot = item.Slot,
                        Price = (int)(item.Price ?? (item.ItemInstance.Item.ReputPrice > 0
                            ? item.ItemInstance.Item.ReputPrice : item.ItemInstance.Item.Price * percent)),
                        RareAmount = item.ItemInstance.Item.Type == (byte)NoscorePocketType.Equipment
                            ? item.ItemInstance.Rare
                            : item.Amount,
                        UpgradeDesign = item.ItemInstance.Item.Type == (byte)NoscorePocketType.Equipment
                            ? item.ItemInstance.Item.IsColored
                                ? item.ItemInstance.Item.Color : item.ItemInstance.Upgrade : (short?)null,
                        VNum = item.ItemInstance.Item.VNum
                    });
                }
            }

            return new NInvPacket
            {
                VisualType = aliveEntity.VisualType,
                VisualId = aliveEntity.VisualId,
                ShopKind = (byte)(percent * 100),
                Unknown = 0,
                Items = shopItemList
            };
        }
    }
}