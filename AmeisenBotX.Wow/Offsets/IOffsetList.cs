using System;

namespace AmeisenBotX.Wow.Offsets
{
    public interface IOffsetList
    {
        nint AuraCount1 { get; }

        nint AuraCount2 { get; }

        nint AuraTable1 { get; }

        nint AuraTable2 { get; }

        nint BattlegroundFinished { get; }

        nint BattlegroundStatus { get; }

        nint BreathTimer { get; }

        nint CameraOffset { get; }

        nint CameraPointer { get; }

        nint ClickToMoveAction { get; }

        nint ClickToMoveDistance { get; }

        nint ClickToMoveGuid { get; }

        nint ClickToMoveTurnSpeed { get; }

        nint ClickToMoveX { get; }

        nint ClientConnection { get; }

        nint ClimbAngle { get; }

        nint CollisionM2C { get; }

        nint CollisionM2S { get; }

        nint CollisionWMO { get; }

        nint ComboPoints { get; }

        nint CorpsePosition { get; }

        nint CurrentlyCastingSpellId { get; }

        nint CurrentlyChannelingSpellId { get; }

        nint CurrentObjectManager { get; }

        nint EndSceneOffset { get; }

        nint EndSceneOffsetDevice { get; }

        nint EndSceneStaticDevice { get; }

        nint FirstObject { get; }

        nint FunctionGameobjectOnRightClick { get; }

        nint FunctionGetActivePlayerObject { get; }

        nint FunctionGetLocalizedText { get; }

        nint FunctionHandleTerrainClick { get; }

        nint FunctionIsOutdoors { get; }

        nint FunctionLuaDoString { get; }

        nint FunctionPlayerClickToMove { get; }

        nint FunctionPlayerClickToMoveStop { get; }

        nint FunctionSetTarget { get; }

        nint FunctionTraceline { get; }

        nint FunctionUnitGetReaction { get; }

        nint FunctionUnitOnRightClick { get; }

        nint FunctionUnitSetFacing { get; }

        nint FunctionWorldFrame { get; }

        nint FunctionWorldRender { get; }

        nint FunctionWorldRenderWorld { get; }

        nint GameState { get; }

        nint IsIngame { get; }

        nint IsWorldLoaded { get; }

        nint LastTargetGuid { get; }

        nint LootWindowOpen { get; }

        nint MapId { get; }

        nint NameBase { get; }

        nint NameMask { get; }

        nint NameStore { get; }

        nint NameString { get; }

        nint NextObject { get; }

        nint PartyLeader { get; }

        nint PartyPlayerGuids { get; }

        nint PetGuid { get; }

        nint PlayerBase { get; }

        nint PlayerGuid { get; }

        nint RaidGroupStart { get; }

        nint RaidLeader { get; }

        nint RenderFlags { get; }

        nint Runes { get; }

        nint RuneType { get; }

        nint TargetGuid { get; }

        nint TickCount { get; }

        nint WowDynobjectPosition { get; }

        nint WowGameobjectPosition { get; }

        nint WowObjectDescriptor { get; }

        nint WowObjectType { get; }

        nint WowUnitDbEntry { get; }

        nint WowUnitDbEntryName { get; }

        nint WowUnitDbEntryType { get; }

        nint WowUnitIsAutoAttacking { get; }

        nint WowUnitPosition { get; }

        nint ZoneId { get; }

        nint ZoneSubText { get; }

        nint ZoneText { get; }

        void Init(nint mainModuleBase);
    }
}