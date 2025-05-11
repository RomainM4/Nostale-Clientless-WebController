using NosCore.NostaleSDK.Item.Rune;
using NosCore.NostaleSDK.Item.Stuff;
using NosCore.Packets.Attributes;
using NosCore.Packets.Enumerations;
using NosCore.Packets.ServerPackets.Auction;
using NostaleSdk.Character;
using NostaleSdk.Item;
using NostaleSdk.Nosbazar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NosCore.Packets.ServerPackets.Auction.RcbListPacket;

namespace NosCore.Packets.CustomPackets.Nosbazar
{
    public class RcBlistCustomPacket
    {
        public long PageIndex { get; set; }

        public List<RcBlistElement?>? Items { get; set; }

        public class RcBlistElement
        {
            public int AuctionId { get; set; }
            public int UserId { get; set; }
            public string UserName { get; set; }
            public int ItemId { get; set; }
            public int Ammount { get; set; }
            public bool IsStacked { get; set; }
            public int Price { get; set; }
            public int MinuteLeft { get; set; }
            public int Unknow1 { get; set; }
            public int InventoryType { get; set; }
            public int ItemRarityType { get; set; }
            public int UpgradeMixedField { get; set; } //depend of the ItemType //upgrade(weapon + armor) / lvl(rune) // stars (pet)
            public int RuneUpgradeCount { get; set; }
            public int Unknow3 { get; set; }
            public IItemInfo? ItemInfoExtend { get; set; }
        }

        public static class RcBlistParser
        {
            public static RcBlistCustomPacket Parse(string RcBlistPacket)
            {
                var RcBlistSplited = RcBlistPacket.Split(" ");

                var RcBlistCustom = new RcBlistCustomPacket { PageIndex = Int32.Parse(RcBlistSplited[1]), Items = new List<RcBlistElement?>() };

                for (int index = 2; index < RcBlistSplited.Length - 1; index++)
                {
                    RcBlistElement buyOffer = new RcBlistElement();
                    var Offer = RcBlistSplited[index];

                    var sp_offer = Offer.Split("|");

                    buyOffer.AuctionId = Int32.Parse(sp_offer[0]);
                    buyOffer.UserId = Int32.Parse(sp_offer[1]);
                    buyOffer.UserName = sp_offer[2];
                    buyOffer.ItemId = Int32.Parse(sp_offer[3]);
                    buyOffer.Ammount = Int32.Parse(sp_offer[4]);
                    buyOffer.IsStacked = Convert.ToBoolean(Int32.Parse(sp_offer[4]));
                    buyOffer.Price = Int32.Parse(sp_offer[6]);
                    buyOffer.MinuteLeft = Int32.Parse(sp_offer[7]);
                    buyOffer.AuctionId = Int32.Parse(sp_offer[8]);//??
                    buyOffer.Unknow1 = Int32.Parse(sp_offer[9]);
                    buyOffer.InventoryType = Int32.Parse(sp_offer[10]);
                    buyOffer.ItemRarityType = Int32.Parse(sp_offer[11]);
                    buyOffer.UpgradeMixedField = Int32.Parse(sp_offer[12]);
                    buyOffer.RuneUpgradeCount = Int32.Parse(sp_offer[13]);
                    //buyOffer.Unknow3 = Int32.Parse(sp_offer[14]);

                    if (sp_offer[14] != string.Empty)
                    {
                        var sp_info = sp_offer[14].Split("^");

                        if (Int32.Parse(sp_info[0]) == (int)ItemInfoType.WeaponClose)
                        {
                            Weapon weapon = new Weapon();

                            weapon.Type = ItemInfoType.WeaponClose;
                            weapon.IconId = Int32.Parse(sp_info[1]);
                            weapon.RarityType = (ItemRarityType)Int32.Parse(sp_info[2]);
                            weapon.UpgradeType = (ItemUpgradeType)Int32.Parse(sp_info[3]);
                            weapon.Fixed = int.Parse(sp_info[4]);
                            weapon.Level = Int32.Parse(sp_info[5]);
                            weapon.DamageMin = Int32.Parse(sp_info[6]);
                            weapon.DamageMax = Int32.Parse(sp_info[7]);
                            weapon.Accuracy = Int32.Parse(sp_info[8]);
                            weapon.CritChance = Int32.Parse(sp_info[9]);
                            weapon.CritDamage = Int32.Parse(sp_info[10]);
                            weapon.AmmoLeft = Int32.Parse(sp_info[11]);
                            weapon.AmmoMax = Int32.Parse(sp_info[12]);
                            weapon.Price = Int32.Parse(sp_info[13]);
                            weapon.Unknow2 = Int32.Parse(sp_info[14]);
                            weapon.RuneRarity = (ItemRarityType)Int32.Parse(sp_info[15]);
                            weapon.RuneHolderPlayerId = Int32.Parse(sp_info[16]);
                            weapon.RuneSlotSize = Int32.Parse(sp_info[17]);

                            if (weapon.RuneSlotSize > 0)
                            {
                                weapon.Options = new List<RuneOption>();
                                for (int i = 18; i < 18 + weapon.RuneSlotSize; i++)
                                {

                                    var sp_rune = sp_info[i].Split(".");
                                    weapon.Options.Add(new RuneOption
                                    {
                                        GradeType = (RuneGradeType)Int32.Parse(sp_rune[0]),
                                        OptionType = (RuneOptionType)Int32.Parse(sp_rune[1]),
                                        Value = Int32.Parse(sp_rune[2])
                                    });
                                }
                            }

                            if (sp_info.Length > 18 + weapon.RuneSlotSize)
                            {
                                RuneUpgrade runeUpgrade = new RuneUpgrade();

                                runeUpgrade.Upgrade = Int32.Parse(sp_info[18 + weapon.RuneSlotSize + 1]);
                                runeUpgrade.IsDamaged = Convert.ToBoolean(Int32.Parse(sp_info[18 + weapon.RuneSlotSize + 2]));
                                runeUpgrade.UpgradeSize = Int32.Parse(sp_info[18 + weapon.RuneSlotSize + 3]);
                                runeUpgrade.Upgrades = new List<RuneUpgradeOption>();

                                for (int i = 18 + weapon.RuneSlotSize + 4; i < 18 + weapon.RuneSlotSize + 4 + runeUpgrade.UpgradeSize; i++)
                                {
                                    var sp = sp_info[i].Split(".");

                                    runeUpgrade.Upgrades.Add(new RuneUpgradeOption
                                    {
                                        Option = Int32.Parse(sp[0]),
                                        Unknow1 = Int32.Parse(sp[1]),
                                        Unknow2 = Int32.Parse(sp[0]),
                                        BuffId = Int32.Parse(sp[0]),
                                        Unknow3 = Int32.Parse(sp[0])
                                    });

                                }

                                weapon.RuneUpgrade = runeUpgrade;
                            }

                            buyOffer.ItemInfoExtend = weapon;
                        }


                    }

                    RcBlistCustom.Items.Add(buyOffer);
                }

                return RcBlistCustom;
            }
        }
    }
}
