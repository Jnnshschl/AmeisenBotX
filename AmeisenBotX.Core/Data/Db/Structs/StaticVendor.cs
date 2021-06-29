using AmeisenBotX.Common.Math;

namespace AmeisenBotX.Core.Data.Db.Structs
{
    public struct StaticVendor : ILikeUnit
    {
        public StaticVendor(int entry, int mapId, float posX, float posY, float posZ, bool ammo, bool food, bool poison, bool reagent, bool repairer, bool likesHorde, bool likesAlliance)
        {
            Entry = entry;
            MapId = mapId;
            Position = new Vector3(posX, posY, posZ);
            IsAmmoVendor = ammo;
            IsFoodVendor = food;
            IsPoisonVendor = poison;
            IsReagentVendor = reagent;
            IsRepairer = repairer;
            LikesHorde = likesHorde;
            LikesAlliance = likesAlliance;
        }

        public int Entry { get; set; }

        public bool IsAmmoVendor { get; set; }

        public bool IsFoodVendor { get; set; }

        public bool IsPoisonVendor { get; set; }

        public bool IsReagentVendor { get; set; }

        public bool IsRepairer { get; set; }

        public bool LikesAlliance { get; set; }

        public bool LikesHorde { get; set; }

        public int MapId { get; set; }

        public Vector3 Position { get; set; }
    }
}