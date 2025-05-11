
using NostaleSdk.Item;

namespace NosCore.NostaleSDK.Item.Stuff
{
    public class JewelOption
    {
        public JewelOptionType Type { get; set; }
        public int Level { get; set; }
        public int Value { get; set; }
    }

    public class Accessory : IItemInfo
    {
        public ItemInfoType Type { get; set; }
        public int IconId { get; set; }

        public int Level { get; set; }
        public int OptionLevel { get; set; }
        public int SlotMaxSize { get; set; }
        public int SlotSize { get; set; }
        public int Price { get; set; }
        public List<JewelOption>? Options { get; set; }

        public string AsPacket()
        {
            throw new NotImplementedException();
        }
    }

}