using AmeisenBotX.Common.Math;

namespace AmeisenBotX.Wow.Cache.Structs
{
    public record StaticMailbox : ILikeUnit
    {
        public StaticMailbox(int entry, int mapId, float posX, float posY, float posZ, bool likesHorde, bool likesAlliance)
        {
            Entry = entry;
            MapId = mapId;
            Position = new Vector3(posX, posY, posZ);
            LikesHorde = likesHorde;
            LikesAlliance = likesAlliance;
        }

        public int Entry { get; set; }

        public bool LikesAlliance { get; set; }

        public bool LikesHorde { get; set; }

        public int MapId { get; set; }

        public Vector3 Position { get; set; }
    }
}