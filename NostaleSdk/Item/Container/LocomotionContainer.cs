using NostaleSdk.Item;

namespace NosCore.NostaleSDK.Item.Container
{
    public class LocomotionContainer : IItemInfo
    {
        public ItemInfoType Type { get; set; }
        public int IconId { get; set; }

        public bool IsFilled { get; set; }
        public int LocomotionId { get; set; }

        public string AsPacket()
        {
            throw new NotImplementedException();
        }

    }
}
