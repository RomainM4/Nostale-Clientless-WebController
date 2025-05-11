using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NostaleSdk.Nosbazar
{
    public enum MedailType
    {
        None,
        Silver,
        Gold
    }

    public enum CategoryMainType
    {
        None = 0,
        MainWeapon,
        MainArmor,
        MainBaseStuff,
        MainAccessory,
        MainSpecialist, // CARE SECONDARY_TYPE 0-30 then 32-35 40-43 46-49 !!
        MainPet,
        MainPartner,
        MainRune,
        MainPrincipal,
        MainConso,
        MainEtc,
        MainLoco
    }

    public enum CategoryClassType
    {
        None = 0,
        Swordman,
        Bowman,
        Wizard,
        Adventurer,
        Martial
    }

    public enum CategoryLevelType
    {
        None = 0,
        Level1,
        Level11,
        Level21,
        Level31,
        Level41,
        Level51,
        Level61,
        Level71,
        Level81,
        Level91,
        LevelHeroAll, // CARE NOT USED FOR ALL
        LeveHero1,
        LeveHero21,
        LeveHero31,
        LeveHero41,
        LeveHero51,
        LeveHero61,
        LeveHero71
    }

    public enum CategoryRarityType
    {
        None = 0,
        RarityNoraml,
        RarityUsefull,
        RarityGood,
        RarityHightQuality,
        RarityExcelent,
        RarityAncestral,
        RarityMysterious,
        RarityLegendary,
        RarityPhenomenal
    }

    public enum CategoryUpgradeType
    {
        None = 0,
        Upgrade0,
        Upgrade1,
        Upgrade2,
        Upgrade3,
        Upgrade4,
        Upgrade5,
        Upgrade6,
        Upgrade7,
        Upgrade8,
        Upgrade9,
        Upgrade10
    }

    public enum CategorySortType
    {
        SortAsc,
        SortDsc,
        SortSmallQantity,
        SortQantity
    }

    public enum SellSearchType
    {
        None = 0,
        ForSale,
        ClosedSale,
        ExpiredSale
    }

    public enum SellStateType
    {
        Listed,
        Sold,
        Expired
    }

}
