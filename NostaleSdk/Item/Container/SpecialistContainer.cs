using NostaleSdk.Item;

namespace NosCore.NostaleSDK.Item.Container
{

    public class SpecialistContainer : IItemInfo
    {
        public ItemInfoType Type { get; set; }
        public int IconId { get; set; }

        public bool IsFilled { get; set; }
        public int SpId { get; set; }
        public int Level { get; set; }
        public int CurrentXp { get; set; }
        public int NextLevelXp { get; set; }
        public int Upgrade { get; set; }
        public int AttackPoint { get; set; }
        public int DefensePoint { get; set; }
        public int ElementPoint { get; set; }
        public int HpMpPoint { get; set; }
        public int Unknown1 { get; set; }
        public int UpgradeGems { get; set; }
        public int FireResistence { get; set; }
        public int WaterResistence { get; set; }
        public int LightResistence { get; set; }
        public int DarkResistence { get; set; }
        public int AttackPointUpgrade { get; set; }
        public int DefensePointUpgrade { get; set; }
        public int ElementPointUpgrade { get; set; }
        public int HpMpPointUpgrade { get; set; }
        public int FireResistenceUpgrade { get; set; }
        public int WaterResistenceUpgrade { get; set; }
        public int LightResistenceUpgrade { get; set; }
        public int DarkResistenceUpgrade { get; set; }

        public string AsPacket()
        {
            throw new NotImplementedException();
        }
    }

}