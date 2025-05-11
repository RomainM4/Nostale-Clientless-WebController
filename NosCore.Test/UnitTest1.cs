using NosCore.Packets.ClientPackets.Battle;
using NosCore.Packets.ClientPackets.Bazaar;
using NosCore.Packets.ClientPackets.Chat;
using NosCore.Packets.ClientPackets.Commands;
using NosCore.Packets.ClientPackets.Families;
using NosCore.Packets.ClientPackets.Inventory;
using NosCore.Packets.ClientPackets.Login;
using NosCore.Packets.ClientPackets.Movement;
using NosCore.Packets.ClientPackets.Npcs;
using NosCore.Packets.ClientPackets.Relations;
using NosCore.Packets.ClientPackets.Shops;
using NosCore.Packets.ClientPackets.Warehouse;
using NosCore.Packets.Interfaces;
using NosCore.Packets;
using NosCore.Packets.ServerPackets.CharacterSelectionScreen;
using NosCore.Packets.ServerPackets.Entities;
using NosCore.Packets.ServerPackets.Login;
using NosCore.Packets.ServerPackets.Relations;
using NosCore.Packets.ServerPackets.UI;
using NosCore.Packets.ServerPackets.Auction;

namespace NosCore.Test
{
    [TestClass]
    public class UnitTest1
    {
        static readonly IDeserializer Deserializer = new Deserializer(
    new[] {
                typeof(NsTestPacket),
                typeof(NsTeStSubPacket),
                typeof(WorldCharacterCount),
                typeof(ClistPacket),
                typeof(MinilandInvitePacket),
                typeof(WhisperPacket),
                typeof(UseItemPacket),
                typeof(MShopPacket),
                typeof(SitPacket),
                typeof(SitSubPacket),
                typeof(MShopItemSubPacket),
                typeof(StPacket),
                typeof(NoS0575Packet),
                typeof(NoS0577Packet),
                typeof(FamilyChatPacket),
                typeof(FStashEndPacket),
                typeof(RequestNpcPacket),
                typeof(NrunPacket),
                typeof(NcifPacket),
                typeof(FinsPacket),
                typeof(DlgPacket),
                typeof(GidxPacket),
                typeof(FinfoPacket),
                typeof(FinfoSubPackets),
                typeof(ClientVersionSubPacket),
                typeof(CBListPacket),
                typeof(RcbListPacket),
                typeof(CScalcPacket),
                typeof(CreateFamilyPacket),
                typeof(UnresolvedPacket)
    });


        [TestMethod]
        public void PacketRCBlistTest()
        {
            var packet = (RcbListPacket)Deserializer.Deserialize("rc_blist 0 3411824|2914451|Miniimec|1|1|0|3000000|11289|2|0|7|0|0|0|0^1^7^0^0^1^28^36^24^4^70^0^100^70^-1^0^0^0 3444060|1600960|JeannotX|2|1|0|499999|20014|2|0|6|0|0|0|0^2^6^0^0^4^35^49^38^4^70^0^100^400^-1^0^0^0 3450877|2502780|Xury|2|1|0|500000|22427|2|0|7|0|0|0|0^2^7^0^0^4^43^57^36^4^70^0^100^400^-1^0^0^0 ");
            Assert.AreEqual("Miniimec", packet.Items.First().OwnerName);
        }

        [TestMethod]
        public void PacketClistTest()
        {
            var packet = (ClistPacket)Deserializer.Deserialize("clist 0 gorlik 0 1 0 9 0 0 1 0 -1.12.1.8.-1.-1.-1.-1.-1.-1 1  1 1 -1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1. 0 0");
            Assert.AreEqual("gorlik", packet.Name);
        }
    }
}