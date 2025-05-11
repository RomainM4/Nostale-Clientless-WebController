using FluentResults;
using Microsoft.Net.Http.Headers;
using NosCore.Packets.CustomPackets.Nosbazar;
using NostaleSdk.Nosbazar;
using NostaleSdk.Packet;
using System.Net.Http;

namespace NosCore.Web.Data.HttpClients
{
    public class ControllerApiClient : IControllerApiClient
    {
        private readonly HttpClient _httpClient;
        public ControllerApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<Result> OpenNosbazarAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Auction>> SearchNosbazarAsync()
        {
            var AuctionList = new List<Auction>();

           var ObservedPacket = await _httpClient.GetFromJsonAsync<NoscoreObservedPacket>("/client/nosbazar/search");

            if(ObservedPacket != null)
            {
                return null;
            }

            var Auction = RcBlistCustomPacket.RcBlistParser.Parse(ObservedPacket.ReceivePacket);

            foreach (var item in Auction.Items)
            {
                AuctionList.Add(new Auction
                {
                    AuctionId = item.AuctionId,
                    UserId = item.UserId,
                    UserName = item.UserName,
                    Ammount = item.Ammount,
                    IsStacked = item.IsStacked,
                    ItemId = item.ItemId,
                    MinuteLeft = item.MinuteLeft,
                    Price = item.Price
                });
            }

            throw new NotImplementedException();
        }
    }
}
