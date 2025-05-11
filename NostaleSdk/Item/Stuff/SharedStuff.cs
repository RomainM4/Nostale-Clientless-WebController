using NostaleSdk.Item;
using NosCore.NostaleSDK.Item.Rune;

namespace NosCore.NostaleSDK.Item.Stuff
{
    public class SharedStuff : IItemInfo
    {
        public ItemInfoType Type { get; set; }
        public int IconId { get; set; }
        public int Level { get; set; }
        public int DefenseClose { get; set; }
        public int DefenseDistance { get; set; }
        public int DefenseMagic { get; set; }
        public int Dodge { get; set; }

        public int FireResistence { get; set; }
        public int WaterResistence { get; set; }
        public int LightResistence { get; set; }
        public int DarkResistence { get; set; }

        public int Price { get; set; }
        public int Combination { get; set; }
        public int RuneSlotSize { get; set; }
        public int ExpireTime { get; set; }


        public string AsPacket()
        {
            throw new NotImplementedException();
        }
    }

}