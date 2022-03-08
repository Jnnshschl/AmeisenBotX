using AmeisenBotX.Wow.Offsets;
using System;

namespace AmeisenBotX.Wow335a.Offsets
{
    public class OffsetList335a : IOffsetList
    {
        public IntPtr AuraCount1 { get; } = new(0xDD0);

        public IntPtr AuraCount2 { get; } = new(0xC54);

        public IntPtr AuraTable1 { get; } = new(0xC50);

        public IntPtr AuraTable2 { get; } = new(0xC58);

        public IntPtr BattlegroundFinished { get; } = new(0xBEA588);

        public IntPtr BattlegroundStatus { get; } = new(0xBEA4D0);

        public IntPtr BreathTimer { get; } = new(0xBD0BA0);

        public IntPtr CameraOffset { get; } = new(0x7E20);

        public IntPtr CameraPointer { get; } = new(0xB7436C);

        public IntPtr CollisionM2C { get; } = new(0x7A50CF);

        public IntPtr CollisionM2S { get; } = new(0x7A52EC);

        public IntPtr CollisionWMO { get; } = new(0x7AE7EA);

        public IntPtr ClickToMoveAction { get; } = new(0xCA11D8 + 0x1C);

        public IntPtr ClickToMoveDistance { get; } = new(0xCA11D8 + 0xC);

        public IntPtr ClickToMoveEnabled { get; } = new(0x30);

        public IntPtr ClickToMoveGuid { get; } = new(0xCA11D8 + 0x20);

        public IntPtr ClickToMovePointer { get; } = new(0xBD08F4);

        public IntPtr ClickToMoveTurnSpeed { get; } = new(0xCA11D8 + 0x4);

        public IntPtr ClickToMoveX { get; } = new(0xCA11D8 + 0x8C);

        public IntPtr ClientConnection { get; } = new(0xC79CE0);

        public IntPtr ClimbAngle { get; } = new(0x858);

        public IntPtr ComboPoints { get; } = new(0xBD084D);

        public IntPtr CorpsePosition { get; } = new(0xBD0A58);

        public IntPtr CurrentlyCastingSpellId { get; } = new(0xA6C);

        public IntPtr CurrentlyChannelingSpellId { get; } = new(0xA80);

        public IntPtr CurrentObjectManager { get; } = new(0x2ED0);

        public IntPtr EndSceneOffset { get; } = new(0xA8); // maybe use 0xAC, clear function, leads to many crashes

        public IntPtr EndSceneOffsetDevice { get; } = new(0x397C);

        public IntPtr EndSceneStaticDevice { get; } = new(0xC5DF88);

        public IntPtr FirstObject { get; } = new(0xAC);

        public IntPtr FunctionGameobjectOnRightClick { get; } = new(0x711140);

        public IntPtr FunctionGetActivePlayerObject { get; } = new(0x4038F0);

        public IntPtr FunctionGetLocalizedText { get; } = new(0x7225E0);

        public IntPtr FunctionHandleTerrainClick { get; } = new(0x80C340);

        public IntPtr FunctionIsOutdoors { get; } = new(0x71B7F0);

        public IntPtr FunctionLuaDoString { get; } = new(0x819210);

        public IntPtr FunctionPlayerClickToMove { get; } = new(0x727400);

        public IntPtr FunctionPlayerClickToMoveStop { get; } = new(0x72B3A0);

        public IntPtr FunctionSetTarget { get; } = new(0x524BF0);

        public IntPtr FunctionTraceline { get; } = new(0x7A3B70);

        public IntPtr FunctionUnitGetReaction { get; } = new(0x7251C0);

        public IntPtr FunctionUnitOnRightClick { get; } = new(0x731260);

        public IntPtr FunctionUnitSetFacing { get; } = new(0x72EA50);

        public IntPtr FunctionWorldFrame { get; } = new(0x4FA390);

        public IntPtr FunctionWorldRender { get; } = new(0x4F8EA0);

        public IntPtr FunctionWorldRenderWorld { get; } = new(0x4FAF90);

        public IntPtr GameState { get; } = new(0xB6A9E0);

        public IntPtr IsIngame { get; } = new(0xBEBAA4);

        public IntPtr IsWorldLoaded { get; } = new(0xBEBA40);

        public IntPtr LastTargetGuid { get; } = new(0xBD07B8);

        public IntPtr LootWindowOpen { get; } = new(0xBFA8D8);

        public IntPtr MapId { get; } = new(0xADFBC4);

        public IntPtr NameBase { get; } = new(0x1C);

        public IntPtr NameMask { get; } = new(0x24);

        public IntPtr NameStore { get; } = new(0xC5D940);

        public IntPtr NameString { get; } = new(0x20);

        public IntPtr NextObject { get; } = new(0x3C);

        public IntPtr PartyLeader { get; } = new(0xBD1968);

        public IntPtr PartyPlayerGuids { get; } = new(0xBD1948);

        public IntPtr PetGuid { get; } = new(0xC234D0);

        public IntPtr PlayerBase { get; } = new(0xD38AE4);

        public IntPtr PlayerGuid { get; } = new(0xCA1238);

        public IntPtr RaidGroupStart { get; } = new(0xBEB568);

        public IntPtr RaidLeader { get; } = new(0xBD1990);

        public IntPtr RenderFlags { get; } = new(0xCD774C);

        public IntPtr Runes { get; } = new(0xC24388);

        public IntPtr RuneType { get; } = new(0xC24304);

        public IntPtr TargetGuid { get; } = new(0xBD07B0);

        public IntPtr TickCount { get; } = new(0xB499A4);

        public IntPtr WowDynobjectPosition { get; } = new(0xE8);

        public IntPtr WowGameobjectPosition { get; } = new(0x1D8);

        public IntPtr WowObjectDescriptor { get; } = new(0x8);

        public IntPtr WowObjectType { get; } = new(0x14);

        public IntPtr WowUnitFlyFlags { get; } = new(0x44);

        public IntPtr WowUnitFlyFlagsPointer { get; } = new(0xD8);

        public IntPtr WowUnitIsAutoAttacking { get; } = new(0xA20);

        public IntPtr WowUnitName1 { get; } = new(0x964);

        public IntPtr WowUnitName2 { get; } = new(0x5C);

        public IntPtr WowUnitPosition { get; } = new(0x798);

        public IntPtr WowUnitSwimFlags { get; } = new(0xA30);

        public IntPtr ZoneId { get; } = new(0xBD080C);

        public IntPtr ZoneSubText { get; } = new(0xBD0784);

        public IntPtr ZoneText { get; } = new(0xBD0788);

        public void Init(IntPtr mainModuleBase)
        {
            // unused, ASLR not enabled
        }
    }
}