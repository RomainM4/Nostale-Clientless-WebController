using NostaleSdk.Packet;
using NostaleSdk.Packet.Sent;

namespace NosCore.NostaleSDK
{
    public static class PacketGenerator
    {
        public static List<NoscoreObservablePacket> ConvertPacketsToObservablePackets(List<string> packets, int delay)
        {
            var PacketObversables = new List<NoscoreObservablePacket>();

            foreach (var item in packets)
            {
                PacketObversables.Add(new NoscoreObservablePacket
                {
                    Value = item,
                    ObservedPackets = new List<string>(),
                    WaitTimeMs = delay,
                    ObservationTimeoutMs = 1000
                });
            }

            return PacketObversables;
        }

        public static List<string> TeleportToNosville()
        {
            List<string> list = new List<string>();

            list.Add(GenerateScript(200));
            list.Add("#guri^1000^1^81^33");
            list.Add(GenerateCClose(1));
            list.Add(GenerateCClose(0));
            list.Add(GenerateBpClose());
            list.Add(GenerateFStashEnd());
            list.Add(GenerateCClose(1));
            list.Add(GenerateCClose(0));

            return list;
        }

        public static List<string> GoToMarcketPlaceFromNosvilleTpPoint()
        {
            List<string> list = new List<string>();

            list.Add(GenerateWalk(81, 37));
            list.Add(GenerateWalk(81, 41, 0));
            list.Add(GenerateWalk(81, 45, 0));
            list.Add(GenerateWalk(81, 46));
            list.Add(GenerateWalk(81, 50, 0));
            list.Add(GenerateWalk(81, 54, 0));
            list.Add(GenerateWalk(81, 58));
            list.Add(GenerateWalk(81, 61));
            list.Add(GenerateWalk(81, 65, 0));
            list.Add(GenerateWalk(80, 66, 0));
            list.Add("preq");
            list.Add(GenerateCClose(1));
            list.Add(GenerateCClose(0));
            list.Add(GenerateBpClose());
            list.Add(GenerateFStashEnd());
            list.Add(GenerateCClose(1));
            list.Add(GenerateCClose(1));

            return list;
        }

        public static List<string> GoToNosbazarNpc()
        {
            List<string> list = new List<string>();

            list.Add(GenerateWalk(18, 6, 0));
            list.Add(GenerateWalk(18, 7));
            list.Add(GenerateWalk(18, 11, 0));
            list.Add(GenerateWalk(18, 15, 0));
            list.Add(GenerateWalk(18, 19));
            list.Add(GenerateWalk(15, 22));
            list.Add(GenerateWalk(12, 25));
            list.Add(GenerateWalk(11, 26));

            return list;
        }

        public static string GenerateWalk(int x, int y, int direction = 1, int unused0 = 11)
        {
            return new WalkPacket {X = x, Y = y, Direction = direction, Unused0 = unused0}.PacketToString();

        }
        public static string GenerateScript(int value, int unused0 = 1)
        {
            return new ScriptPacket { Unused0 = unused0, Value = value }.PacketToString();
        }

        public static string GenerateCClose(byte type)
        {
            return new CClosePacket { Type = type }.PacketToString();
        }

        public static string GenerateSelect(byte slot)
        {
            return new SelectPacket { Slot = slot }.PacketToString();
        }

        public static string GenerateLbs(byte type)
        {
            return new LbsPacket { Type = type }.PacketToString();
        }

        public static string GenerateNpInfo(byte page)
        {
            return new NpInfoPacket { Page = page }.PacketToString();
        }

        public static string GenerateBpClose()
        {
            return "bp_close";
        }

        public static string GenerateGameStart()
        {
            return "game_start";
        }

        public static string GenerateFStashEnd()
        {
            return "f_stash_end";
        }
    }
}
