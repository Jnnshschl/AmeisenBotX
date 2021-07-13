namespace AmeisenBotX.Core.Fsm.Enums
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
        Combat,
        Eating,
        LoadingScreen,
        InsideAoeDamage,
        Unstuck,
        Looting,
        Repairing,
        Selling,
        Battleground,
        Job,
        Dungeon,
        Questing,
        Grinding,
        StateTalkToQuestgivers,
    }
}