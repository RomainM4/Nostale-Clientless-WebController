using NostaleSdk.Item;

namespace NosCore.NostaleSDK.Item.Container
{
    public class SpecialistPartnerSkill
    {
        public int SpellId { get; set; }
        public PartnerSpRankType SpRankType { get; set; }
    }

    public class SpecialistPartnerContainer : IItemInfo
    {
        public ItemInfoType Type { get; set; }
        public int IconId { get; set; }

        public bool IsFilled { get; set; }
        public int SpId { get; set; }
        public ElementType ElementType { get; set; }
        public List<SpecialistPartnerSkill>? Skills { get; set; }
        public int Upgrade { get; set; }
        public int AttackUpgrade { get; set; }
        public int DefenseUpgrade { get; set; }
        public int CritReduction { get; set; }
        public int HpMpUpgrade { get; set; }
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
