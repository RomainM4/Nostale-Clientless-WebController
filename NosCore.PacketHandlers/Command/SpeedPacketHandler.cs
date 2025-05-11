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
using NosCore.GameObject;
using NosCore.GameObject.ComponentEntities.Extensions;
using NosCore.GameObject.Networking.ClientSession;
using NosCore.Packets.Enumerations;
using System.Threading.Tasks;

namespace NosCore.PacketHandlers.Command
{
    public class SpeedPacketHandler : PacketHandler<SpeedPacket>, IWorldPacketHandler
    {
        public override Task ExecuteAsync(SpeedPacket speedPacket, ClientSession session)
        {
            if ((speedPacket.Speed <= 0) || (speedPacket.Speed >= 60))
            {
                return session.SendPacketAsync(session.Character.GenerateSay(speedPacket.Help(), SayColorType.Yellow));
            }

            session.Character.VehicleSpeed = speedPacket.Speed >= 60 ? (byte)59 : speedPacket.Speed;
            return session.SendPacketAsync(session.Character.GenerateCond());
        }
    }
}