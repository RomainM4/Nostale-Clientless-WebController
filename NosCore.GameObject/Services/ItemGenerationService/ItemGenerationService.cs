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

using Mapster;
using NosCore.Data.Dto;
using NosCore.Data.Enumerations;
using NosCore.Data.Enumerations.I18N;
using NosCore.Data.Enumerations.Items;
using NosCore.Data.StaticEntities;
using NosCore.GameObject.Services.EventLoaderService;
using NosCore.GameObject.Services.InventoryService;
using NosCore.GameObject.Services.ItemGenerationService.Item;
using NosCore.Packets.ClientPackets.Inventory;
using NosCore.Shared.I18N;
using Serilog;
using System;
using System.Collections.Generic;

namespace NosCore.GameObject.Services.ItemGenerationService
{
    public class ItemGenerationService : IItemGenerationService
    {
        private readonly IEventLoaderService<Item.Item, Tuple<InventoryItemInstance, UseItemPacket>>? _runner;
        private readonly List<ItemDto> _items;
        private readonly ILogger _logger;
        private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage = null!;

        public ItemGenerationService(List<ItemDto> items,
            EventLoaderService<Item.Item, Tuple<InventoryItemInstance, UseItemPacket>, IUseItemEventHandler> runner, ILogger logger, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
        {
            _items = items;
            _logger = logger;
            _runner = runner;
            _logLanguage = logLanguage;
        }

        public IItemInstance Convert(IItemInstanceDto k)
        {
            IItemInstance item = k switch
            {
                BoxInstanceDto _ => k.Adapt<BoxInstance>(),
                SpecialistInstanceDto _ => k.Adapt<SpecialistInstance>(),
                WearableInstanceDto _ => k.Adapt<WearableInstance>(),
                UsableInstanceDto _ => k.Adapt<UsableInstance>(),
                _ => k.Adapt<ItemInstance>()
            };
            var itemDto = _items.Find(s => s.VNum == k.ItemVNum);
            if (itemDto == null)
            {
                throw new InvalidOperationException(_logLanguage[LogLanguageKey.UNBOUND_ITEM_DETECTED]);
            }
            item.Item = itemDto.Adapt<Item.Item>();
            _runner?.LoadHandlers(item.Item);

            return item;
        }

        public IItemInstance Create(short itemToCreateVNum)
        {
            return Create(itemToCreateVNum, 1);
        }

        public IItemInstance Create(short itemToCreateVNum, short amount)
        {
            return Create(itemToCreateVNum, amount, 0);
        }

        public IItemInstance Create(short itemToCreateVNum, short amount, sbyte rare)
        {
            return Create(itemToCreateVNum, amount, rare, 0);
        }

        public IItemInstance Create(short itemToCreateVNum, short amount, sbyte rare, byte upgrade)
        {
            return Create(itemToCreateVNum, amount, rare, upgrade, 0);
        }

        public IItemInstance Create(short itemToCreateVNum, short amount, sbyte rare, byte upgrade,
            byte design)
        {
            var item = Generate(itemToCreateVNum, amount, rare, upgrade, design);
            if (item.Item != null)
            {
                _runner?.LoadHandlers(item.Item);
            }
            return item;
        }

        public IItemInstance Generate(short itemToCreateVNum, short amount, sbyte rare, byte upgrade,
            byte design)
        {
            var itemToCreate = _items.Find(s => s.VNum == itemToCreateVNum)!.Adapt<Item.Item>();
            switch (itemToCreate.Type)
            {
                case NoscorePocketType.Miniland:
                    return new ItemInstance(itemToCreate)
                    {
                        Amount = amount,
                        DurabilityPoint = itemToCreate.MinilandObjectPoint / 2
                    };

                case NoscorePocketType.Equipment:
                    switch (itemToCreate.ItemType)
                    {
                        case ItemType.Specialist:
                            return new SpecialistInstance(itemToCreate)
                            {
                                SpLevel = 1,
                                Amount = amount,
                                Design = design,
                                Upgrade = upgrade
                            };
                        case ItemType.Box:
                            return new BoxInstance(itemToCreate)
                            {
                                Amount = amount,
                                Upgrade = upgrade,
                                Design = design
                            };
                        default:
                            var wear = new WearableInstance(itemToCreate, _logger, _logLanguage)
                            {
                                Amount = amount,
                                Rare = rare,
                                Upgrade = upgrade,
                                Design = design
                            };
                            if (wear.Rare > 0)
                            {
                                wear.SetRarityPoint();
                            }

                            return wear;
                    }

                default:
                    return new ItemInstance(itemToCreate)
                    {
                        Amount = amount
                    };
            }
        }
    }
}