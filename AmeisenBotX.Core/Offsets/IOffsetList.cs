using System;

namespace AmeisenBotX.Core.Offsets
{
    public interface IOffsetList
    {
        IntPtr BattlegroundFinished { get; }

        IntPtr BattlegroundStatus { get; }

        IntPtr CameraOffset { get; }

        IntPtr CameraPointer { get; }

        IntPtr CharacterSlotSelected { get; }

        IntPtr ClickToMoveAction { get; }

        IntPtr ClickToMoveDistance { get; }

        IntPtr ClickToMoveEnabled { get; }

        IntPtr ClickToMoveGuid { get; }

        IntPtr ClickToMovePointer { get; }

        IntPtr ClickToMoveTurnSpeed { get; }

        IntPtr ClickToMoveX { get; }

        IntPtr ClientConnection { get; }

        IntPtr ComboPoints { get; }

        IntPtr CorpsePosition { get; }

        IntPtr CurrentlyCastingSpellId { get; }

        IntPtr CurrentlyChannelingSpellId { get; }

        IntPtr CurrentObjectManager { get; }

        IntPtr CvarMaxFps { get; }

        IntPtr EndSceneOffset { get; }

        IntPtr EndSceneOffsetDevice { get; }

        IntPtr EndSceneStaticDevice { get; }

        IntPtr FirstObject { get; }

        IntPtr FunctionGameobjectOnRightClick { get; }

        IntPtr FunctionGetActivePlayerObject { get; }

        IntPtr FunctionGetLocalizedText { get; }

        IntPtr FunctionHandleTerrainClick { get; }

        IntPtr FunctionLuaDoString { get; }

        IntPtr FunctionPlayerClickToMove { get; }

        IntPtr FunctionPlayerClickToMoveStop { get; }

        IntPtr FunctionSetTarget { get; }

        IntPtr FunctionTraceline { get; }

        IntPtr FunctionUnitGetReaction { get; }

        IntPtr FunctionUnitOnRightClick { get; }

        IntPtr FunctionUnitSendMovementPacket { get; }

        IntPtr FunctionUnitSetFacing { get; }

        IntPtr FunctionWorldFrame { get; }

        IntPtr FunctionWorldRender { get; }

        IntPtr FunctionWorldRenderWorld { get; }

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

        IntPtr PetGuid { get; }

        IntPtr PlayerBase { get; }

        IntPtr PlayerGuid { get; }

        IntPtr PlayerName { get; }

        IntPtr RaidGroupPlayer { get; }

        IntPtr RaidGroupStart { get; }

        IntPtr RaidLeader { get; }

        IntPtr Runes { get; }

        IntPtr RuneType { get; }

        IntPtr TargetGuid { get; }

        IntPtr TickCount { get; }

        IntPtr WowGameobjectPosition { get; }

        IntPtr WowObjectDescriptor { get; }

        IntPtr WowObjectPosition { get; }

        IntPtr WowObjectType { get; }

        IntPtr WowUnitIsAutoAttacking { get; }

        IntPtr WowUnitName1 { get; }

        IntPtr WowUnitName2 { get; }

        IntPtr WowUnitPosition { get; }

        IntPtr WowUnitRotation { get; }

        IntPtr ZoneId { get; }

        IntPtr ZoneSubText { get; }

        IntPtr ZoneText { get; }
    }
}