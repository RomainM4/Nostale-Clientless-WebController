using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NostaleSdk.Item
{
    public enum ItemInfoType
    {
        WeaponClose = 0, // epee + dague + baton + gants + marque
        WeaponDistance, //bow + crossbow + gun + catapulte
        Armor, // armor + costume
        BaseStuff, // gants + chapeau cost + arme cost + ...
        Accessory, // accessoire  + fée + minifamiller + amulette
        WeaponMagic,
        Unknown6,
        Container, // Sp + fammilier
        Raid,
        Rune,
        Unknown10,
        Loco, // loco perle
        Fairy,
        SpPartner,
    }

    public enum ItemRarityType
    {
        Noraml = 0,
        Usefull,
        Good,
        HightQuality,
        Excelent,
        Ancestral,
        Mysterious,
        Legendary,
        Phenomenal
    }

    public enum ItemUpgradeType
    {
        Up0 = 0,
        Up1,
        Up2,
        Up3,
        Up4,
        Up5,
        Up6,
        Up7,
        Up8,
        Up9,
        Up10

    }

    public enum ElementType
    {
        Fire,
        Water,
        Light,
        Dark
    };

    public enum JewelOptionType
    {
        Hp,
        Mp,
        Recovery,
        MpRecovery,
        MpConsumption,
        CriticalDamageDecrease
    }

    public enum RuneOptionType
    {
        None,
        DamageUp,
        DamagePercentUp,
        BleedingSmall,
        BleedingMedium,
        BleedingLarge,
        Stun,     // syncope
        Freeze,
        StunLarge, // syncope mortelle
        DamagePlants,
        DamageAnimals,
        DamageMonsters,
        DamageZombies,
        DamageAnimalsSmall,
        DamageBossMap,
        CriticalChance,
        CriticalDamage,
        CastUnstopable,
        ElemFire,
        ElemWater,
        ElemLight,
        ElemDark,
        ElemAll,
        ManaConso,
        HealthRecoverKill,
        ManaRecoverKill,
        SlDamage,
        SlDefense,
        SlElem,
        SlHealthMana,
        SlAll,
        GoldUp,
        ExpUp,
        ExpWorkUp,
        PvpDamageUp,
        PvpDefenseBreak,
        PvpRezFireBreak,
        PvpRezWaterBreak,
        PvpRezLightBreak,
        PvpRezDarkBreak,
        PvpRezAllBreak,
        PvpNoMiss,
        PvpChanceDamageUp, //Avec une probabilité de 15%%, les dégats en PvP augmentent
        PvpManaDrain,
        PvpRezFireIgnore, //La réistance au Feu est ignoré en PvP avec une probabilité de 25 %.
        PvpRezWaterIgnore,
        PvpRezLightIgnore,
        PvpRezDarkIgnore,
        SpPointRecoverKill,
        AccuracyUp,
        FocusUp,
        DefenseCloseUp,
        DefenseDistanceUp,
        DefenseMagicUp,
        DefenseAllUp,
        BleedingSmallChanceDec,
        BleedingMediumChanceDec,
        BleedingHightChanceDec,
        StunChanceDec,
        StunAllChanceDec,
        HandOfDeadChanceDec,
        FreezeChanceDec,
        BlindChanceDec,
        HindranceChanceDec, //entrave
        DefenseBreakChanceDec,
        ShockChanceDec, // choc
        ParalysisPoisonChanceDec,
        NegativEffectChanceDec,
        HealthRecoverRest,
        HealthRecoverNatural,
        ManaRecoverRest,
        ManaRecoverNatural,
        HealthRecoverDefense,
        CriticalChanceDec,
        RezFireUp,
        RezWaterUp,
        RezLightUp,
        RezDarkUp,
        RezAllUp,
        DignityLostDec,
        ProductivityPointDec,
        ProductivityUp,
        FoodRecoverUp,
        PvpDefenseAllUp,
        PvpDodgeCloseUp,
        PvpDodgeDistanceUp,
        PvpMagicDamageIgnore,
        PvpDodgeAllUp,
        PvpManaProtect,
        PvpFireImunity,
        PvpWaterImunity,
        PvpLightImunity,
        PvpDarkImunity,
        AbsorbClose,
        AbsorbDistance,
        DodgeUp
    }

    public enum RuneGradeType
    {
        Unused,
        C,
        B,
        A,
        S,
        CPrincipalWeapon,
        BPrincipalWeapon,
        APrincipalWeapon,
        SPrincipalWeapon,
        CPvp,
        BPvp,
        APvp,
        SPvp,
        Unused1,
        Unused2,
        Unused3,
        Unused4,
        Unused5,
        Unused6,
        Unused7,
        Unused8,
        Unused9,
        Unused10,
        Unused11,
        Unused12

    }

    public enum PartnerSpRankType
    {
        None,
        RankF,
        RankE,
        RankD,
        RankC,
        RankB,
        RankA,
        RankS
    }
}
