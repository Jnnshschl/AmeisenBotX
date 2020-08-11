namespace AmeisenBotX.Core.Movement.Pathfinding.Objects
{
    public struct RandomPointRequest
    {
        public RandomPointRequest(int mapId, Vector3 a, float maxRadius)
        {
            A = a;
            MaxRadius = maxRadius;
            MapId = mapId;
        }

        public Vector3 A { get; set; }

        public int MapId { get; set; }

        public float MaxRadius { get; set; }
    }
}