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
using NosCore.Data.Dto;
using NosCore.Data.Enumerations.I18N;
using NosCore.Data.WebApi;
using NosCore.GameObject.Holders;
using NosCore.Packets.Enumerations;
using NosCore.Shared.I18N;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NosCore.GameObject.InterChannelCommunication.Hubs.ChannelHub;
using NosCore.GameObject.InterChannelCommunication.Hubs.PubSub;
using NosCore.Shared.Enumerations;


namespace NosCore.GameObject.Services.FriendService
{
    public class FriendService(ILogger logger, IDao<CharacterRelationDto, Guid> characterRelationDao,
            IDao<CharacterDto, long> characterDao, FriendRequestHolder friendRequestHolder,
            IPubSubHub pubSubHub, IChannelHub channelHub, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
        : IFriendService
    {
        public async Task<LanguageKey> AddFriendAsync(long characterId, long secondCharacterId, FinsPacketType friendsPacketType)
        {
            var servers = await channelHub.GetCommunicationChannels();
            var accounts = await pubSubHub.GetSubscribersAsync();
            var character = accounts.FirstOrDefault(s => s.ConnectedCharacter?.Id == characterId && servers.Where(c => c.Type == ServerType.WorldServer).Any(x => x.Id == s.ChannelId));
            var targetCharacter = accounts.FirstOrDefault(s => s.ConnectedCharacter?.Id == secondCharacterId && servers.Where(c => c.Type == ServerType.WorldServer).Any(x => x.Id == s.ChannelId));

            var friendRequest = friendRequestHolder.FriendRequestCharacters.Where(s =>
                (s.Value.Item2 == character?.ConnectedCharacter?.Id) &&
                (s.Value.Item1 == targetCharacter?.ConnectedCharacter?.Id)).ToList();
            if ((character != null) && (targetCharacter != null))
            {
                if (character.ChannelId != targetCharacter.ChannelId)
                {
                    throw new ArgumentException();
                }

                var relations = characterRelationDao.Where(s => s.CharacterId == characterId)?.ToList() ?? new List<CharacterRelationDto>();
                if (relations.Count(s => s.RelationType == CharacterRelationType.Friend) >= 80)
                {
                    return LanguageKey.FRIENDLIST_FULL;
                }

                if (relations.Any(s =>
                    (s.RelationType == CharacterRelationType.Blocked) &&
                    (s.RelatedCharacterId == secondCharacterId)))
                {
                    return LanguageKey.BLACKLIST_BLOCKED;
                }

                if (relations.Any(s =>
                    (s.RelationType == CharacterRelationType.Friend) &&
                    (s.RelatedCharacterId == secondCharacterId)))
                {
                    return LanguageKey.ALREADY_FRIEND;
                }

                if (character.ConnectedCharacter!.FriendRequestBlocked ||
                    targetCharacter.ConnectedCharacter!.FriendRequestBlocked)
                {
                    return LanguageKey.FRIEND_REQUEST_BLOCKED;
                }

                if (!friendRequest.Any())
                {
                    friendRequestHolder.FriendRequestCharacters[Guid.NewGuid()] =
                        new Tuple<long, long>(character.ConnectedCharacter.Id,
                            targetCharacter.ConnectedCharacter.Id);
                    return LanguageKey.FRIEND_REQUEST_SENT;
                }

                switch (friendsPacketType)
                {
                    case FinsPacketType.Accepted:
                        var data = new CharacterRelationDto
                        {
                            CharacterId = character.ConnectedCharacter.Id,
                            RelatedCharacterId = targetCharacter.ConnectedCharacter.Id,
                            RelationType = CharacterRelationType.Friend
                        };

                        await characterRelationDao.TryInsertOrUpdateAsync(data).ConfigureAwait(false);
                        var data2 = new CharacterRelationDto
                        {
                            CharacterId = targetCharacter.ConnectedCharacter.Id,
                            RelatedCharacterId = character.ConnectedCharacter.Id,
                            RelationType = CharacterRelationType.Friend
                        };

                        await characterRelationDao.TryInsertOrUpdateAsync(data2).ConfigureAwait(false);
                        friendRequestHolder.FriendRequestCharacters.TryRemove(friendRequest.First().Key, out _);
                        return LanguageKey.FRIEND_ADDED;
                    case FinsPacketType.Rejected:
                        friendRequestHolder.FriendRequestCharacters.TryRemove(friendRequest.First().Key, out _);
                        return LanguageKey.FRIEND_REJECTED;
                    default:
                        logger.Error(logLanguage[LogLanguageKey.INVITETYPE_UNKNOWN]);
                        friendRequestHolder.FriendRequestCharacters.TryRemove(friendRequest.First().Key, out _);
                        throw new ArgumentException();
                }
            }

            friendRequestHolder.FriendRequestCharacters.TryRemove(friendRequest.First().Key, out _);
            throw new ArgumentException();
        }

        public async Task<List<CharacterRelationStatus>> GetFriendsAsync(long id)
        {
            var charList = new List<CharacterRelationStatus>();
            var list = characterRelationDao
                .Where(s => (s.CharacterId == id) && (s.RelationType != CharacterRelationType.Blocked));
            if (list == null)
            {
                return charList;
            }
            foreach (var rel in list)
            {
                var servers = await channelHub.GetCommunicationChannels();
                var accounts = await pubSubHub.GetSubscribersAsync();
                var character = accounts.FirstOrDefault(s => s.ConnectedCharacter?.Id == rel.RelatedCharacterId && servers.Where(c => c.Type == ServerType.WorldServer).Any(x => x.Id == s.ChannelId));
          
                charList.Add(new CharacterRelationStatus
                {
                    CharacterName = (await characterDao.FirstOrDefaultAsync(s => s.CharacterId == rel.RelatedCharacterId).ConfigureAwait(false))?.Name,
                    CharacterId = rel.RelatedCharacterId,
                    IsConnected = character != null,
                    RelationType = rel.RelationType,
                    CharacterRelationId = rel.CharacterRelationId
                });
            }

            return charList;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var rel = await characterRelationDao.FirstOrDefaultAsync(s =>
                (s.CharacterRelationId == id) && (s.RelationType == CharacterRelationType.Friend)).ConfigureAwait(false);
            if (rel == null)
            {
                return false;
            }
            var rel2 = await characterRelationDao.FirstOrDefaultAsync(s =>
                (s.CharacterId == rel.RelatedCharacterId) && (s.RelatedCharacterId == rel.CharacterId) &&
                (s.RelationType == CharacterRelationType.Friend)).ConfigureAwait(false);
            if (rel2 == null)
            {
                return false;
            }
            await characterRelationDao.TryDeleteAsync(rel.CharacterRelationId).ConfigureAwait(false);
            await characterRelationDao.TryDeleteAsync(rel2.CharacterRelationId).ConfigureAwait(false);
            return true;
        }
    }
}