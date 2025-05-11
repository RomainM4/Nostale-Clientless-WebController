using NostaleSdk.Item;

namespace NosCore.NostaleSDK.Item.Container
{
    public class HouseHoldContainer : IItemInfo
    {
        public ItemInfoType Type { get; set; }
        public int IconId { get; set; }

        public bool IsFilled { get; set; }
        public int HouseHoldId { get; set; }
        public int Level { get; set; }
        public int CurrentXp { get; set; }
        public int NextLevelXp { get; set; }
        public int AttackUpgrade { get; set; }
        public int DefenseUpgrade { get; set; }
        public int Stars { get; set; }


        public string AsPacket()
        {
            throw new NotImplementedException();
        }

    }
}
