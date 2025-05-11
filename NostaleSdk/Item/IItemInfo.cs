using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NostaleSdk.Item
{
    public interface IItemInfo
    {
        public ItemInfoType Type { get; set; }
        public int IconId { get; set; }

        public string AsPacket();
    }
}
