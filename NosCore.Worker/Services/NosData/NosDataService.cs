using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NosCore.Worker.Services.NosData
{
    public record Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Item(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

    }
    public class NosDataService : INosDataService
    {
        public const int MAX_NOSBAZAR_ID_SEARCH = 47; 
        public const string ITEM_DATA_PATH = "C:\\Users\\admin\\Desktop\\Projects\\Nostale-Projects\\NosCore_Web\\NosCore.Worker\\Shared\\Item_Data.bin";
        public const string ItemCodePathEN = "C:\\Users\\admin\\Desktop\\Projects\\Nostale-Projects\\NosCore_Web\\NosCore.Worker\\Shared\\Item_Code_EN.bin";
        public const string ITEM_CODE_PATH_FR = "C:\\Users\\admin\\Desktop\\Projects\\Nostale-Projects\\NosCore_Web\\NosCore.Worker\\Shared\\Item_Code_FR.bin";

        private List<Item> ItemsList = new List<Item>();
        private bool _IsInitialized = false;
        public string GetItemNameFromId(int id)
        {
            Item? item = ItemsList.Find(item => item.Id == id);

            if (item != null)
                return item.Name;

            return "Unknow Name";
        }

        public string GetItemDescriptionFromId(int id)
        {
            Item? item = ItemsList.Find(item => item.Id == id);

            if (item != null)
                return item.Description;

            return "Unknow Description";
        }

        public List<int> GetItemIdsFromRequest(string request)
        {
            List<int> Ids = new List<int>();

            Ids = ItemsList.Where(item => item.Name.ToLower().Contains(request.ToLower())).Select(j => j.Id).ToList();        

            return Ids;
        }

        public bool IsInitialized()
        {
            return _IsInitialized;
        }

        private string ExtractData(string content, string begin, string end)
        {
            var IndexBegin = content.IndexOf(begin);
            if (IndexBegin == -1)
                return "";

            var IndexEnd = content.IndexOf(end, IndexBegin + begin.Length);

            return content.Substring(IndexBegin + begin.Length, IndexEnd - (IndexBegin + begin.Length));
        }

        private string ExtractDataDescription(string content, string begin, string end)
        {
            var IndexBegin = content.IndexOf(begin) + 3;
            if (IndexBegin == -1)
                return "";

            var IndexEnd = content.IndexOf(end, IndexBegin + begin.Length + 3);

            return content.Substring(IndexBegin + begin.Length, IndexEnd - (IndexBegin + begin.Length));
        }

        public void GenerateData(string itemDataPath, string itemCodeLangagePath)
        {
            var ItemsData = File.ReadAllText(itemDataPath);
            var ItemsCode = File.ReadAllText(itemCodeLangagePath);

            var ItemDataList = ItemsData.Split("#========================================================");
            var ItemCodeList = ItemsCode.Split("\r");

            int i = 0;
            foreach (var ItemData in ItemDataList)
            {
                if (ItemData == "\r~\r")
                    continue;

                var Id = ExtractData(ItemData, "VNUM" + (char)0x9, "" + (char)0x9);
                var NameCode = ExtractData(ItemData, "NAME" + (char)0x9, "" + (char)0x0D);
                var DescriptionCode = ExtractDataDescription(ItemData, "LINEDESC", "" + (char)0x0D);

                if (i == 6714)
                {
                    var yy = 0;
                }

                i++;
                var Name = "";
                var Description = "";

                foreach (var ItemCode in ItemCodeList)
                {
                    var sp = ItemCode.Split((char)0x9);
                    if (sp.Length < 2)
                        continue;

                    if (sp[0] == NameCode)
                        Name = sp[1];

                    if (sp[0] == DescriptionCode)
                        Description = sp[1];

                    if (Name != "" && Description != "")
                        break;
                }

                ItemsList.Add(new Item(int.Parse(Id), Name, Description));
            }
            _IsInitialized = true;
        }

    }
}
