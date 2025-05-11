using NostaleSdk.Item;

namespace NosCore.NostaleSDK.Item.Rune
{
    public class RuneOption
    {
        public RuneGradeType GradeType { get; set; }
        public RuneOptionType OptionType { get; set; }
        public int Value { get; set; }
    }

    public class RuneUpgradeOption
    {
        public int Option {  get; set; }
        public int Unknow1 { get; set; }
        public int Unknow2 { get; set; }
        public int BuffId { get; set; }
        public int Unknow3 { get; set; }

    }

    public class RuneUpgrade
    {
        public int Upgrade { get; set; }
        public bool IsDamaged { get; set; }
        public int UpgradeSize { get; set; }

        public List<RuneUpgradeOption> Upgrades { get; set; }
    }


    public class Rune : IItemInfo
    {
        public ItemInfoType Type { get; set; }
        public int IconId { get; set; }

        public int Level { get; set; }
        public int Price { get; set; }
        public int SlotSize {  get; set; }
        public List<RuneOption> Options { get; set; } = new List<RuneOption>();
        public RuneUpgrade? Upgrade { get; set; }
        public string AsPacket()
        {
            throw new NotImplementedException();
        }
    }
}