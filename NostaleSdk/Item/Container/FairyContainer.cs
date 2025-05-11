using NostaleSdk.Item;

namespace NosCore.NostaleSDK.Item.Container
{
    public class FairyContainer : IItemInfo
    {
        public ItemInfoType Type { get; set; }
        public int IconId { get; set; }

        public bool IsFilled { get; set; }
        public int FairyId { get; set; }
        public int Level { get; set; }

        public string AsPacket()
        {
            throw new NotImplementedException();
        }

    }
}
