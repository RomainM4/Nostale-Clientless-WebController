using NostaleSdk.Item;
using NosCore.NostaleSDK.Item.Rune;

namespace NosCore.NostaleSDK.Item.Stuff
{

    public class Armor : IItemInfo
    {
        public ItemInfoType Type { get; set; }
        public int IconId { get; set; }

        public ItemRarityType RarityType { get; set; }
        public ItemUpgradeType UpgradeType { get; set; }
        public int Unknow1 { get; set; }
        public int Level { get; set; }
        public int DefenseClose { get; set; }
        public int DefenseDistance { get; set; }
        public int DefenseMagic { get; set; }
        public int Dodge { get; set; }
        public int Price { get; set; }
        public int ExpireTime { get; set; }
        public ItemRarityType RuneRarity { get; set; }
        public int RuneHolderPlayerId { get; set; }
        public int RuneSlotSize { get; set; }
        public List<RuneOption>? Options { get; set; }
        public RuneUpgrade? Upgrade { get; set; }


        public string AsPacket()
        {
            throw new NotImplementedException();
        }
    }

}