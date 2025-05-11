using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NosCore.Worker.Services.NosData
{
    public interface INosDataService
    {
        string GetItemNameFromId(int id);
        string GetItemDescriptionFromId(int id);
        List<int> GetItemIdsFromRequest(string request);
        void GenerateData(string itemDataPath, string itemCodeLangagePath);
        bool IsInitialized();
    }
}
