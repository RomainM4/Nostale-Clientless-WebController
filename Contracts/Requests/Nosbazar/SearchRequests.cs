using NostaleSdk.Nosbazar;


namespace Contracts.Requests.Nosbazar
{
    public record SearchRequest
    {
        public int PageSearch { get; set; }
        public CategoryMainType MainType { get; set; }
        public CategoryClassType ClassType { get; set; }
        public CategoryLevelType LevelType { get; set; }
        public CategoryRarityType RarityType { get; set; }
        public CategoryUpgradeType UpgradeType { get; set; }
        public CategorySortType SortType { get; set; }
    }

}
