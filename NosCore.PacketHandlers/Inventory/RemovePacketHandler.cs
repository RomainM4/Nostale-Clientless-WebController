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
using NosCore.GameObject;
using NosCore.GameObject.ComponentEntities.Extensions;
using NosCore.GameObject.Networking.ClientSession;
using NosCore.Packets.ClientPackets.Inventory;
using NosCore.Packets.ClientPackets.Specialists;
using NosCore.Packets.Enumerations;
using NosCore.Packets.ServerPackets.UI;
using System.Threading.Tasks;
using NosCore.Networking;


namespace NosCore.PacketHandlers.Inventory
{
    public class RemovePacketHandler : PacketHandler<RemovePacket>, IWorldPacketHandler
    {
        public override async Task ExecuteAsync(RemovePacket removePacket, ClientSession clientSession)
        {
            var inventory =
                clientSession.Character.InventoryService.LoadBySlotAndType((short)removePacket.InventorySlot,
                    NoscorePocketType.Wear);
            if (inventory == null)
            {
                return;
            }

            if (inventory.ItemInstance!.Item!.EquipmentSlot == EquipmentType.Sp)
            {
                await clientSession.HandlePacketsAsync(new[] { new SpTransformPacket
                {
                    Type = SlPacketType.WearSpAndTransform
                } });
            }

            var inv = clientSession.Character.InventoryService.MoveInPocket((short)removePacket.InventorySlot,
                NoscorePocketType.Wear, NoscorePocketType.Equipment);

            if (inv == null)
            {
                await clientSession.SendPacketAsync(new MsgiPacket
                {
                    Type = MessageType.Default,
                    Message = Game18NConstString.NotEnoughSpace
                }).ConfigureAwait(false);
                return;
            }

            await clientSession.SendPacketAsync(inv.GeneratePocketChange((PocketType)inv.Type, inv.Slot)).ConfigureAwait(false);

            await clientSession.Character.MapInstance.SendPacketAsync(clientSession.Character.GenerateEq()).ConfigureAwait(false);
            await clientSession.SendPacketAsync(clientSession.Character.GenerateEquipment()).ConfigureAwait(false);

            if (inv.ItemInstance!.Item!.EquipmentSlot == EquipmentType.Fairy)
            {
                await clientSession.Character.MapInstance.SendPacketAsync(
                    clientSession.Character.GeneratePairy(null)).ConfigureAwait(false);
            }
        }
    }
}