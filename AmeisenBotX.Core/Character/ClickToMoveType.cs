namespace AmeisenBotX.Core.Character
{
    public enum ClickToMoveType : int
    {
        None = 0,
        FaceTarget = 1,
        FaceDestination = 2,
        Stop = 3,
        Move = 4,
        Interact = 5,
        Loot = 6,
        InteractObject = 7,
        FaceOther = 8,
        Skin = 9,
        AttackPos = 10,
        AttackGuid = 11,
        Attack = 16,
        WalkAndRotate = 19
    }
}