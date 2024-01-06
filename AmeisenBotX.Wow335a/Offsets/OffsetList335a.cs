using AmeisenBotX.Wow.Offsets;
using System;

namespace AmeisenBotX.Wow335a.Offsets
{
    public class OffsetList335a : IOffsetList
    {
        public nint AuraCount1 { get; } = new(0xDD0);

        public nint AuraCount2 { get; } = new(0xC54);

        public nint AuraTable1 { get; } = new(0xC50);

        public nint AuraTable2 { get; } = new(0xC58);

        public nint BattlegroundFinished { get; } = new(0xBEA588);

        public nint BattlegroundStatus { get; } = new(0xBEA4D0);

        public nint BreathTimer { get; } = new(0xBD0BA0);

        public nint CameraOffset { get; } = new(0x7E20);

        public nint CameraPointer { get; } = new(0xB7436C);

        public nint ClickToMoveAction { get; } = new(0xCA11D8 + 0x1C);

        public nint ClickToMoveDistance { get; } = new(0xCA11D8 + 0xC);

        public nint ClickToMoveEnabled { get; } = new(0x30);

        public nint ClickToMoveGuid { get; } = new(0xCA11D8 + 0x20);

        public nint ClickToMovePointer { get; } = new(0xBD08F4);

        public nint ClickToMoveTurnSpeed { get; } = new(0xCA11D8 + 0x4);

        public nint ClickToMoveX { get; } = new(0xCA11D8 + 0x8C);

        public nint ClientConnection { get; } = new(0xC79CE0);

        public nint ClimbAngle { get; } = new(0x858);

        public nint CollisionM2C { get; } = new(0x7A50CF);

        public nint CollisionM2S { get; } = new(0x7A52EC);

        public nint CollisionWMO { get; } = new(0x7AE7EA);

        public nint ComboPoints { get; } = new(0xBD084D);

        public nint CorpsePosition { get; } = new(0xBD0A58);

        public nint CurrentlyCastingSpellId { get; } = new(0xA6C);

        public nint CurrentlyChannelingSpellId { get; } = new(0xA80);

        public nint CurrentObjectManager { get; } = new(0x2ED0);

        public nint EndSceneOffset { get; } = new(0xA8); // maybe use 0xAC, clear function, leads to many crashes

        public nint EndSceneOffsetDevice { get; } = new(0x397C);

        public nint EndSceneStaticDevice { get; } = new(0xC5DF88);

        public nint FirstObject { get; } = new(0xAC);

        public nint FunctionGameobjectOnRightClick { get; } = new(0x711140);

        public nint FunctionGetActivePlayerObject { get; } = new(0x4038F0);

        public nint FunctionGetLocalizedText { get; } = new(0x7225E0);

        public nint FunctionHandleTerrainClick { get; } = new(0x80C340);

        public nint FunctionIsOutdoors { get; } = new(0x71B7F0);

        public nint FunctionLuaDoString { get; } = new(0x819210);

        public nint FunctionPlayerClickToMove { get; } = new(0x727400);

        public nint FunctionPlayerClickToMoveStop { get; } = new(0x72B3A0);

        public nint FunctionSetTarget { get; } = new(0x524BF0);

        public nint FunctionTraceline { get; } = new(0x7A3B70);

        public nint FunctionUnitGetReaction { get; } = new(0x7251C0);

        public nint FunctionUnitOnRightClick { get; } = new(0x731260);

        public nint FunctionUnitSetFacing { get; } = new(0x72EA50);

        public nint FunctionWorldFrame { get; } = new(0x4FA390);

        public nint FunctionWorldRender { get; } = new(0x4F8EA0);

        public nint FunctionWorldRenderWorld { get; } = new(0x4FAF90);

        public nint GameState { get; } = new(0xB6A9E0);

        public nint IsIngame { get; } = new(0xBEBAA4);

        public nint IsWorldLoaded { get; } = new(0xBEBA40);

        public nint LastTargetGuid { get; } = new(0xBD07B8);

        public nint LootWindowOpen { get; } = new(0xBFA8D8);

        public nint MapId { get; } = new(0xADFBC4);

        public nint NameBase { get; } = new(0x1C);

        public nint NameMask { get; } = new(0x24);

        public nint NameStore { get; } = new(0xC5D940);

        public nint NameString { get; } = new(0x20);

        public nint NextObject { get; } = new(0x3C);

        public nint PartyLeader { get; } = new(0xBD1968);

        public nint PartyPlayerGuids { get; } = new(0xBD1948);

        public nint PetGuid { get; } = new(0xC234D0);

        public nint PlayerBase { get; } = new(0xD38AE4);

        public nint PlayerGuid { get; } = new(0xCA1238);

        public nint RaidGroupStart { get; } = new(0xBEB568);

        public nint RaidLeader { get; } = new(0xBD1990);

        public nint RenderFlags { get; } = new(0xCD774C);

        public nint Runes { get; } = new(0xC24388);

        public nint RuneType { get; } = new(0xC24304);

        public nint TargetGuid { get; } = new(0xBD07B0);

        public nint TickCount { get; } = new(0xB499A4);

        public nint WowDynobjectPosition { get; } = new(0xE8);

        public nint WowGameobjectPosition { get; } = new(0x1D8);

        public nint WowObjectDescriptor { get; } = new(0x8);

        public nint WowObjectType { get; } = new(0x14);

        public nint WowUnitDbEntry { get; } = new(0x964);

        public nint WowUnitDbEntryName { get; } = new(0x5C);

        public nint WowUnitDbEntryType { get; } = new(0x10);

        public nint WowUnitIsAutoAttacking { get; } = new(0xA20);

        public nint WowUnitPosition { get; } = new(0x798);

        public nint ZoneId { get; } = new(0xBD080C);

        public nint ZoneSubText { get; } = new(0xBD0784);

        public nint ZoneText { get; } = new(0xBD0788);

        public void Init(nint mainModuleBase)
        {
            // unused, ASLR not enabled
        }
    }
}