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

using Json.More;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using NosCore.Dao.Interfaces;
using NosCore.Data.Dto;
using NosCore.Data.Enumerations.Bazaar;
using NosCore.Data.Enumerations.I18N;
using NosCore.Data.Enumerations.Items;
using NosCore.Data.WebApi;
using NosCore.GameObject.Holders;
using NosCore.Packets.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NosCore.GameObject.Services.BazaarService
{
    public class BazaarService(BazaarItemsHolder holder, IDao<BazaarItemDto, long> bazaarItemDao,
            IDao<IItemInstanceDto?, Guid> itemInstanceDao, IClock clock)
        : IBazaarService
    {
        public List<BazaarLink> GetBazaar(long id, byte? index, byte? pageSize, BazaarListType? typeFilter,
            byte? subTypeFilter, byte? levelFilter, byte? rareFilter, byte? upgradeFilter, long? sellerFilter)
        {
            var bzlist = new List<BazaarLink>();
#pragma warning disable CS0219 // Variable is assigned but its value is never used
            var applyRareFilter = false;
            var applyUpgradeFilter = false;
            var applySpLevelFilter = false;
            var applyLevelFilter = false;
            var applyClassFilter = false;
            PocketType? pocketType = null;
            ItemType? itemType = null;
            byte? subtypeFilter = null;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
            IEnumerable<BazaarLink> bzlinks;
            if (id != -1)
            {
                bzlinks = holder.BazaarItems.Values.Where(s => s.BazaarItem?.BazaarItemId == id);
            }
            else
            {
                bzlinks = holder.BazaarItems.Values.Where(s =>
                    (s.BazaarItem?.SellerId == sellerFilter) || (sellerFilter == null));
            }

            foreach (var bz in bzlinks)
            {
                switch (typeFilter)
                {
                    case BazaarListType.Weapon:
                        itemType = ItemType.Weapon;
                        pocketType = PocketType.Equipment;
                        applyClassFilter = true;
                        applyLevelFilter = true;
                        applyRareFilter = true;
                        applyUpgradeFilter = true;
                        break;

                    case BazaarListType.Armor:
                        itemType = ItemType.Armor;
                        pocketType = PocketType.Equipment;
                        applyClassFilter = true;
                        applyLevelFilter = true;
                        applyRareFilter = true;
                        applyUpgradeFilter = true;
                        break;

                    case BazaarListType.Equipment:
                        itemType = ItemType.Fashion;
                        pocketType = PocketType.Equipment;
                        if (subTypeFilter > 0)
                        {
                            var equipmentTypeFilter = (BazaarEquipmentType)subTypeFilter;
                        }

                        applyLevelFilter = true;
                        break;

                    case BazaarListType.Jewelery:
                        itemType = ItemType.Jewelery;
                        pocketType = PocketType.Equipment;
                        if (subTypeFilter > 0)
                        {
                            var jeweleryTypeFilter = (BazaarJeweleryType)subTypeFilter;
                        }

                        applyLevelFilter = true;
                        break;

                    case BazaarListType.Specialist:
                    case BazaarListType.Npc:
                    case BazaarListType.Pet:
                        pocketType = PocketType.Equipment;
                        itemType = ItemType.Box;
                        applySpLevelFilter = true;
                        break;

                    case BazaarListType.Shell:
                        pocketType = PocketType.Equipment;
                        itemType = ItemType.Shell;
                        applySpLevelFilter = true;
                        if (subTypeFilter > 0)
                        {
                            subtypeFilter = subTypeFilter++;
                            applyRareFilter = true;
                        }

                        break;

                    case BazaarListType.Main:
                        pocketType = PocketType.Main;
                        if (subTypeFilter > 0)
                        {
                            var mainTypeFilter = (BazaarMainType)subTypeFilter;
                        }

                        break;

                    case BazaarListType.Usable:
                        pocketType = PocketType.Etc;
                        if (subTypeFilter > 0)
                        {
                            var bazaarTypeFilter = (BazaarUsableType)subTypeFilter;
                        }

                        break;

                    case BazaarListType.Other:
                        pocketType = PocketType.Equipment;
                        itemType = ItemType.Box;
                        break;

                    case BazaarListType.Vehicle:
                        itemType = ItemType.Box;
                        break;
                }

                bzlist.Add(bz);
            }

            //todo this need to be move to the filter when done
            return bzlist.Skip(index ?? 0 * pageSize ?? 0).Take((byte)(pageSize ?? bzlist.Count)).ToList();
        }

        public async Task<bool> DeleteBazaarAsync(long id, short count, string requestCharacterName)
        {
            var bzlink = holder.BazaarItems.Values.FirstOrDefault(s => s.BazaarItem?.BazaarItemId == id);
            if (bzlink == null)
            {
                throw new ArgumentException();
            }

            if ((bzlink.ItemInstance?.Amount - count < 0) || (count < 0))
            {
                return false;
            }

            if ((bzlink.ItemInstance?.Amount == count) && (requestCharacterName == bzlink.SellerName))
            {
                await bazaarItemDao.TryDeleteAsync(bzlink.BazaarItem!.BazaarItemId).ConfigureAwait(false);
                holder.BazaarItems.TryRemove(bzlink.BazaarItem.BazaarItemId, out _);
                await itemInstanceDao.TryDeleteAsync(bzlink.ItemInstance.Id).ConfigureAwait(false);
            }
            else
            {
                var item = (IItemInstanceDto)bzlink.ItemInstance!;
                item.Amount -= count;
                await itemInstanceDao.TryInsertOrUpdateAsync(item).ConfigureAwait(false);
            }

            return true;
        }

        public async Task<LanguageKey> AddBazaarAsync(Guid itemInstanceId, long characterId, string? characterName, bool hasMedal, long price, bool isPackage, short duration, short amount)
        {
            var items = holder.BazaarItems.Values.Where(o => o.BazaarItem!.SellerId == characterId);
            if (items.Count() > 10 * (hasMedal ? 10 : 1) - 1)
            {
                return LanguageKey.LIMIT_EXCEEDED;
            }

            var item = await itemInstanceDao.FirstOrDefaultAsync(s => s!.Id == itemInstanceId).ConfigureAwait(true);
            if ((item == null) || (item.Amount < amount) || (amount < 0) ||
                (price < 0))
            {
                throw new ArgumentException();
            }

            Guid itemId;
            if (item.Amount == amount)
            {
                itemId = item.Id;
            }
            else
            {
                itemId = item.Id;
                item.Amount -= amount;
                item = await itemInstanceDao.TryInsertOrUpdateAsync(item).ConfigureAwait(true);
                item!.Id = Guid.NewGuid();
            }

            item = await itemInstanceDao.TryInsertOrUpdateAsync(item).ConfigureAwait(true);

            var bazaarItem = new BazaarItemDto
            {
                Amount = amount,
                DateStart = clock.GetCurrentInstant(),
                Duration = duration,
                IsPackage = isPackage,
                MedalUsed = hasMedal,
                Price = price,
                SellerId = characterId,
                ItemInstanceId = item!.Id
            };
            bazaarItem = await bazaarItemDao.TryInsertOrUpdateAsync(bazaarItem).ConfigureAwait(true);
            holder.BazaarItems.TryAdd(bazaarItem.BazaarItemId,
                new BazaarLink
                {
                    BazaarItem = bazaarItem, SellerName = characterName,
                    ItemInstance = (ItemInstanceDto)item
                });

            return LanguageKey.OBJECT_IN_BAZAAR;
        }

        public async Task<BazaarLink?> ModifyBazaarAsync(long id, Json.Patch.JsonPatch bzMod)
        {
            var item = holder.BazaarItems.Values
                .FirstOrDefault(o => o.BazaarItem?.BazaarItemId == id);
            if ((item?.BazaarItem == null) || (item.BazaarItem?.Amount != item.ItemInstance?.Amount))
            {
                return null;
            }

            var result = bzMod.Apply(JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(item, new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb))).RootElement.AsNode());
            item = JsonSerializer.Deserialize<BazaarLink>(result!.Result, new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
            var bz = item!.BazaarItem!;
            await bazaarItemDao.TryInsertOrUpdateAsync(bz).ConfigureAwait(true);
            holder.BazaarItems[item.BazaarItem!.BazaarItemId] = item;
            return item;
        }
    }
}