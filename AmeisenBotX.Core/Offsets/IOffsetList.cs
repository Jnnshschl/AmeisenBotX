using System;

namespace AmeisenBotX.Core.Offsets
{
    public interface IOffsetList
    {
        IntPtr AccountName { get; }

        IntPtr AutolootEnabled { get; }

        IntPtr AutolootPointer { get; }

        IntPtr BattlegroundFinished { get; }

        IntPtr BattlegroundStatus { get; }

        IntPtr CharacterSlotSelected { get; }

        IntPtr ChatBuffer { get; }

        IntPtr ChatNextMessage { get; }

        IntPtr ChatOpened { get; }

        IntPtr ClickToMoveAction { get; }

        IntPtr ClickToMoveDistance { get; }

        IntPtr ClickToMoveEnabled { get; }

        IntPtr ClickToMoveGuid { get; }

        IntPtr ClickToMovePendingMovement { get; }

        IntPtr ClickToMovePointer { get; }

        IntPtr ClickToMoveTurnSpeed { get; }

        IntPtr ClickToMoveX { get; }

        IntPtr ClickToMoveY { get; }

        IntPtr ClickToMoveZ { get; }

        IntPtr ClientConnection { get; }

        IntPtr ComboPoints { get; }

        IntPtr ContinentName { get; }

        IntPtr CorpsePosition { get; }

        IntPtr CurrentlyCastingSpellId { get; }

        IntPtr CurrentlyChannelingSpellId { get; }

        IntPtr CurrentObjectManager { get; }

        IntPtr CvarMaxFps { get; }

        IntPtr EndSceneOffset { get; }

        IntPtr EndSceneOffsetDevice { get; }

        IntPtr EndSceneStaticDevice { get; }

        IntPtr ErrorMessage { get; }

        IntPtr FirstObject { get; }

        IntPtr FunctionCastSpellById { get; }

        IntPtr FunctionGameobjectOnRightClick { get; }

        IntPtr FunctionGetActivePlayerObject { get; }

        IntPtr FunctionGetLocalizedText { get; }

        IntPtr FunctionHandleTerrainClick { get; }

        IntPtr FunctionIsClickMoving { get; }

        IntPtr FunctionLuaDoString { get; }

        IntPtr FunctionObjectGetPosition { get; }

        IntPtr FunctionPlayerClickToMove { get; }

        IntPtr FunctionPlayerClickToMoveStop { get; }

        IntPtr FunctionPlayerIsClickMoving { get; }

        IntPtr FunctionRenderWorld { get; }

        IntPtr FunctionSetTarget { get; }

        IntPtr FunctionSpellGetSpellCooldown { get; }

        IntPtr FunctionUnitGetReaction { get; }

        IntPtr FunctionUnitOnRightClick { get; }

        IntPtr FunctionUnitSendMovementPacket { get; }

        IntPtr FunctionUnitSetFacing { get; }

        IntPtr GameState { get; }

        IntPtr IsWorldLoaded { get; }

        IntPtr LastTargetGuid { get; }

        IntPtr LootWindowOpen { get; }

        IntPtr MapId { get; }

        IntPtr NameBase { get; }

        IntPtr NameMask { get; }

        IntPtr NameStore { get; }

        IntPtr NameString { get; }

        IntPtr NextObject { get; }

        IntPtr PartyLeader { get; }

        IntPtr PartyPlayer1 { get; }

        IntPtr PartyPlayer2 { get; }

        IntPtr PartyPlayer3 { get; }

        IntPtr PartyPlayer4 { get; }

        IntPtr PerformanceCounter { get; }

        IntPtr PetGuid { get; }

        IntPtr PlayerBase { get; }

        IntPtr PlayerGuid { get; }

        IntPtr PlayerName { get; }

        IntPtr RaidGroupPlayer { get; }

        IntPtr RaidGroupStart { get; }

        IntPtr RaidLeader { get; }

        IntPtr RealmName { get; }

        IntPtr RuneCooldown { get; }

        IntPtr Runes { get; }

        IntPtr RuneType { get; }

        IntPtr SpellbookCount { get; }

        IntPtr SpellbookSpells { get; }

        IntPtr TargetGuid { get; }

        IntPtr TickCount { get; }

        IntPtr WowBuild { get; }

        IntPtr WowDynobjectCasterGuid { get; }

        IntPtr WowDynobjectFacing { get; }

        IntPtr WowDynobjectPosition { get; }

        IntPtr WowDynobjectRadius { get; }

        IntPtr WowDynobjectSpellId { get; }

        IntPtr WowGameobjectDisplayId { get; }

        IntPtr WowGameobjectLevel { get; }

        IntPtr WowGameobjectPosition { get; }

        IntPtr WowGameobjectType { get; }

        IntPtr WowObjectDescriptor { get; }

        IntPtr WowObjectEntryId { get; }

        IntPtr WowObjectGuid { get; }

        IntPtr WowObjectPosition { get; }

        IntPtr WowObjectScale { get; }

        IntPtr WowObjectType { get; }

        IntPtr WowUnitIsAutoAttacking { get; }

        IntPtr WowUnitPosition { get; }

        IntPtr WowUnitRotation { get; }

        IntPtr ZoneId { get; }

        IntPtr ZoneSubText { get; }

        IntPtr ZoneText { get; }
    }
}