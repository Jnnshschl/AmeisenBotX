namespace AmeisenBotX.Pathfinding
{
    public struct PathRequest
    {
        public PathRequest(int mapId, WowPosition a, WowPosition b)
        {
            MapId = mapId;
            A = a;
            B = b;
        }

        public WowPosition A { get; set; }
        public WowPosition B { get; set; }
        public int MapId { get; set; }
    }
}