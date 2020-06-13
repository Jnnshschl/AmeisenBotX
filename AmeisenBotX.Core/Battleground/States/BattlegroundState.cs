namespace AmeisenBotX.Core.Battleground.States
{
    public enum BattlegroundState
    {
        WaitingForStart,
        MoveToEnemyBase,
        MoveToOwnBase,
        MoveToEnemyFlagCarrier,
        AssistOwnFlagCarrier,
        DefendMyself,
        PickupEnemyFlag,
        PickupOwnFlag,
        PickupBuff,
        ExitBattleground
    }
}