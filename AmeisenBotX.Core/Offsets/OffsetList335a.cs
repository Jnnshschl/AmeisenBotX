using System;

namespace AmeisenBotX.Core.Offsets
{
    public class OffsetList335a : IOffsetList
    {
        public IntPtr AuraCount1 { get; } = new IntPtr(0xDD0);

        public IntPtr AuraCount2 { get; } = new IntPtr(0xC54);

        public IntPtr AuraTable1 { get; } = new IntPtr(0xC50);

        public IntPtr AuraTable2 { get; } = new IntPtr(0xC58);

        public IntPtr BattlegroundFinished { get; } = new IntPtr(0xBEA588);

        public IntPtr BattlegroundStatus { get; } = new IntPtr(0xBEA4D0);

        public IntPtr BreathTimer { get; } = new IntPtr(0xBD0BA0);

        public IntPtr CameraOffset { get; } = new IntPtr(0x7E20);

        public IntPtr CameraPointer { get; } = new IntPtr(0xB7436C);

        public IntPtr CharacterSlotSelected { get; } = new IntPtr(0xAC436C);

        public IntPtr ClickToMoveAction { get; } = new IntPtr(0xCA11D8 + 0x1C);

        public IntPtr ClickToMoveDistance { get; } = new IntPtr(0xCA11D8 + 0xC);

        public IntPtr ClickToMoveEnabled { get; } = new IntPtr(0x30);

        public IntPtr ClickToMoveGuid { get; } = new IntPtr(0xCA11D8 + 0x20);

        public IntPtr ClickToMovePointer { get; } = new IntPtr(0xBD08F4);

        public IntPtr ClickToMoveTurnSpeed { get; } = new IntPtr(0xCA11D8 + 0x4);

        public IntPtr ClickToMoveX { get; } = new IntPtr(0xCA11D8 + 0x8C);

        public IntPtr ClientConnection { get; } = new IntPtr(0xC79CE0);

        public IntPtr ComboPoints { get; } = new IntPtr(0xBD084D);

        public IntPtr CorpsePosition { get; } = new IntPtr(0xBD0A58);

        public IntPtr CurrentlyCastingSpellId { get; } = new IntPtr(0xA6C);

        public IntPtr CurrentlyChannelingSpellId { get; } = new IntPtr(0xA80);

        public IntPtr CurrentObjectManager { get; } = new IntPtr(0x2ED0);

        public IntPtr CvarMaxFps { get; } = new IntPtr(0xC5DF7C);

        public IntPtr CvarMaxFpsBk { get; } = new IntPtr(0xC5DF7C);

        public IntPtr EndSceneOffset { get; } = new IntPtr(0xA8); // maybe use 0xAC, clear function, leads to many crashes

        public IntPtr EndSceneOffsetDevice { get; } = new IntPtr(0x397C);

        public IntPtr EndSceneStaticDevice { get; } = new IntPtr(0xC5DF88);

        public IntPtr FirstObject { get; } = new IntPtr(0xAC);

        public IntPtr FunctionGameobjectOnRightClick { get; } = new IntPtr(0x711140);

        public IntPtr FunctionGetActivePlayerObject { get; } = new IntPtr(0x4038F0);

        public IntPtr FunctionGetLocalizedText { get; } = new IntPtr(0x7225E0);

        public IntPtr FunctionHandleTerrainClick { get; } = new IntPtr(0x80C340);

        public IntPtr FunctionLuaDoString { get; } = new IntPtr(0x819210);

        public IntPtr FunctionPlayerClickToMove { get; } = new IntPtr(0x727400);

        public IntPtr FunctionPlayerClickToMoveStop { get; } = new IntPtr(0x72B3A0);

        public IntPtr FunctionSetTarget { get; } = new IntPtr(0x524BF0);

        public IntPtr FunctionTraceline { get; } = new IntPtr(0x7A3B70);

        public IntPtr FunctionUnitGetReaction { get; } = new IntPtr(0x7251C0);

        public IntPtr FunctionUnitOnRightClick { get; } = new IntPtr(0x731260);

        public IntPtr FunctionUnitSendMovementPacket { get; } = new IntPtr(0x7413F0);

        public IntPtr FunctionUnitSetFacing { get; } = new IntPtr(0x72EA50);

        public IntPtr FunctionWorldFrame { get; } = new IntPtr(0x4FA390);

        public IntPtr FunctionWorldRender { get; } = new IntPtr(0x4F8EA0);

        public IntPtr FunctionWorldRenderWorld { get; } = new IntPtr(0x4FAF90);

        public IntPtr GameState { get; } = new IntPtr(0xB6A9E0);

        public IntPtr IsWorldLoaded { get; } = new IntPtr(0xBEBA40);

        public IntPtr LastTargetGuid { get; } = new IntPtr(0xBD07B8);

        public IntPtr LootWindowOpen { get; } = new IntPtr(0xBFA8D8);

        public IntPtr MapId { get; } = new IntPtr(0xADFBC4);

        public IntPtr NameBase { get; } = new IntPtr(0x1C);

        public IntPtr NameMask { get; } = new IntPtr(0x24);

        public IntPtr NameStore { get; } = new IntPtr(0xC5D940);

        public IntPtr NameString { get; } = new IntPtr(0x20);

        public IntPtr NextObject { get; } = new IntPtr(0x3C);

        public IntPtr PartyLeader { get; } = new IntPtr(0xBD1968);

        public IntPtr PartyPlayerGuids { get; } = new IntPtr(0xBD1948);

        public IntPtr PetGuid { get; } = new IntPtr(0xC234D0);

        public IntPtr PlayerBase { get; } = new IntPtr(0xD38AE4);

        public IntPtr PlayerGuid { get; } = new IntPtr(0xCA1238);

        public IntPtr PlayerName { get; } = new IntPtr(0xC79D18);

        public IntPtr RaidGroupStart { get; } = new IntPtr(0xBEB568);

        public IntPtr RaidLeader { get; } = new IntPtr(0xBD1990);

        public IntPtr RenderFlags { get; } = new IntPtr(0xCD774C);

        public IntPtr Runes { get; } = new IntPtr(0xC24388);

        public IntPtr RuneType { get; } = new IntPtr(0xC24304);

        public IntPtr TargetGuid { get; } = new IntPtr(0xBD07B0);

        public IntPtr TickCount { get; } = new IntPtr(0xB499A4);

        public IntPtr WowDynobjectPosition { get; } = new IntPtr(0xE8);

        public IntPtr WowGameobjectPosition { get; } = new IntPtr(0x1D8);

        public IntPtr WowObjectDescriptor { get; } = new IntPtr(0x8);

        public IntPtr WowObjectType { get; } = new IntPtr(0x14);

        public IntPtr WowUnitFlyFlags { get; } = new IntPtr(0x44);

        public IntPtr WowUnitFlyFlagsPointer { get; } = new IntPtr(0xD8);

        public IntPtr WowUnitIsAutoAttacking { get; } = new IntPtr(0xA20);

        public IntPtr WowUnitName1 { get; } = new IntPtr(0x964);

        public IntPtr WowUnitName2 { get; } = new IntPtr(0x05C);

        public IntPtr WowUnitPosition { get; } = new IntPtr(0x798);

        public IntPtr WowUnitRotation { get; } = new IntPtr(0x7A8);

        public IntPtr WowUnitSwimFlags { get; } = new IntPtr(0xA30);

        public IntPtr ZoneId { get; } = new IntPtr(0xBD080C);

        public IntPtr ZoneSubText { get; } = new IntPtr(0xBD0784);

        public IntPtr ZoneText { get; } = new IntPtr(0xBD0788);
    }
}