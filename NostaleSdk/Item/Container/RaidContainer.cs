using NostaleSdk.Item;

namespace NosCore.NostaleSDK.Item.Container
{
    public class RaidContainer : IItemInfo
    {
        public ItemInfoType Type { get; set; }
        public int IconId { get; set; }

        public int Unknow { get; set; }
        public ItemRarityType ItemRarityType { get; set; }

        public string AsPacket()
        {
            throw new NotImplementedException();
        }

    }
}
