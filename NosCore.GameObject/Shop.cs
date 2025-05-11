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

using NosCore.Data.Dto;
using NosCore.Data.StaticEntities;
using NosCore.GameObject.Networking.ClientSession;
using System.Collections.Concurrent;
using System.Linq;

namespace NosCore.GameObject
{
    public class Shop : ShopDto
    {
        private int? _size;

        public ConcurrentDictionary<int, ShopItem> ShopItems { get; set; } = new();

        public ClientSession? Session { get; set; }
        public long Sell { get; internal set; }

        public int Size
        {
            get => _size ?? ShopItems.Values.Select(s => s.Slot).DefaultIfEmpty().Max() + 1;
            set => _size = value;
        }
        public I18NString Name { get; set; } = new();
    }
}