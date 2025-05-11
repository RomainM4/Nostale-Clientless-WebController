
using NostaleSdk.Character;
using NostaleSdk.Item;

namespace NostaleSdk.Nosbazar
{
    public class NosbazarSearch
    {
        public int PageSearch { get; set; }
        public CategoryMainType MainType { get; set; }
        public CategoryClassType ClassType { get; set; }
        public CategoryLevelType LevelType { get; set; }
        public CategoryRarityType RarityType { get; set; }
        public CategoryUpgradeType UpgradeType { get; set; }
        public CategorySortType SortType { get; set; }
        public int Reserved0 { get; set; }
        public int IsIdsSearch { get; set; }
        public List<int>? Ids {  get; set; }
    }

    public class Auction
    {
        public int AuctionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "null";
        public int ItemId { get; set; }
        public int Ammount { get; set; }
        public bool IsStacked { get; set; }
        public int Price { get; set; }
        public int MinuteLeft { get; set; }
    }

    internal class Nosbazar
    {
    }
}
