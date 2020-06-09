namespace AmeisenBotX.Core.Statemachine.States
{
    public enum BotState : int
    {
        None,
        StartWow,
        Login,
        Idle,
        Dead,
        Ghost,
        Following,
        Attacking,
        Eating,
        LoadingScreen,
        InsideAoeDamage,
        Unstuck,
        Looting,
        Repairing,
        Selling,
        Battleground,
        Job,
        Dungeon
    }
}