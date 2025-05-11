﻿using System;
using System.Threading.Tasks;
using NosCore.GameObject.Networking.ClientSession;

namespace NosCore.GameObject.Services.MapChangeService
{
    public interface IMapChangeService
    {
        Task ChangeMapInstanceAsync(ClientSession session, Guid mapInstanceId, int? mapX = null, int? mapY = null);
        Task ChangeMapAsync(ClientSession session, short? mapId = null, short? mapX = null, short? mapY = null);
    }
}