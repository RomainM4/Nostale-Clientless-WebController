using NostaleSdk.Item;
using NosCore.NostaleSDK.Item.Rune;

namespace NosCore.NostaleSDK.Item.Stuff
{

    public class Weapon : IItemInfo
    {
        public ItemInfoType Type { get; set; }
        public int IconId { get; set; }

        public ItemRarityType RarityType { get; set; }
        public ItemUpgradeType UpgradeType { get; set; }
        public int Fixed { get; set; }
        public int Level {  get; set; }
        public int DamageMin { get; set; }
        public int DamageMax { get; set; }
        public int Accuracy { get; set; }
        public int CritChance { get; set; }
        public int CritDamage { get; set; }
        public int AmmoLeft { get; set; }
        public int AmmoMax { get; set; }
        public int Price { get; set; }
        public int Unknow2 { get; set; }
        public ItemRarityType RuneRarity { get; set; }
        public int RuneHolderPlayerId { get; set; }
        public int RuneSlotSize { get; set; }
        public List<RuneOption>? Options { get; set; }
        public RuneUpgrade? RuneUpgrade { get; set; }


        public string AsPacket()
        {
            throw new NotImplementedException();
        }
    }

}