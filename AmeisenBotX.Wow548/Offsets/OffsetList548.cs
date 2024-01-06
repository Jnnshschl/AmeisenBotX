using AmeisenBotX.Wow.Offsets;

namespace AmeisenBotX.Wow548.Offsets
{
    public class OffsetList548 : IOffsetList
    {
        public nint AuraCount1 { get; } = new(0x1218);

        public nint AuraCount2 { get; } = new(0xE14);

        public nint AuraTable1 { get; } = new(0xE18);

        public nint AuraTable2 { get; } = new(0xE1C);

        public nint BattlegroundFinished { get; private set; }

        public nint BattlegroundStatus { get; private set; }

        public nint BreathTimer { get; } = new(0x0);

        public nint CameraOffset { get; } = new(0x8208);

        public nint CameraPointer { get; private set; }

        public nint ClickToMoveAction { get; private set; }

        public nint ClickToMoveDistance { get; private set; }

        public nint ClickToMoveGuid { get; private set; }

        public nint ClickToMoveTurnSpeed { get; private set; }

        public nint ClickToMoveX { get; private set; }

        public nint ClientConnection { get; private set; }

        public nint ClimbAngle { get; } = new(0x0);

        public nint CollisionM2C { get; } = new(0x0);

        public nint CollisionM2S { get; } = new(0x0);

        public nint CollisionWMO { get; } = new(0x0);

        public nint ComboPoints { get; private set; }

        public nint CorpsePosition { get; private set; }

        public nint CurrentlyCastingSpellId { get; } = new(0xCB8);

        public nint CurrentlyChannelingSpellId { get; } = new(0xCD0);

        public nint CurrentObjectManager { get; } = new(0x462C);

        public nint EndSceneOffset { get; } = new(0xA8);

        public nint EndSceneOffsetDevice { get; } = new(0x2820);

        public nint EndSceneStaticDevice { get; private set; }

        public nint FirstObject { get; } = new(0xCC);

        public nint FunctionGameobjectOnRightClick { get; } = new(0x0);

        public nint FunctionGetActivePlayerObject { get; private set; }

        public nint FunctionGetLocalizedText { get; private set; }

        public nint FunctionHandleTerrainClick { get; private set; }

        public nint FunctionIsOutdoors { get; private set; }

        public nint FunctionLuaDoString { get; private set; }

        public nint FunctionPlayerClickToMove { get; private set; }

        public nint FunctionPlayerClickToMoveStop { get; } = new(0x0); // unused

        public nint FunctionSetTarget { get; private set; }

        public nint FunctionTraceline { get; private set; }

        public nint FunctionUnitGetReaction { get; private set; }

        public nint FunctionUnitOnRightClick { get; private set; }

        public nint FunctionUnitSetFacing { get; private set; }

        public nint FunctionUnitSetFacingSmooth { get; private set; }

        public nint FunctionWorldFrame { get; } = new(0x0);

        public nint FunctionWorldRender { get; } = new(0x0);

        public nint FunctionWorldRenderWorld { get; } = new(0x0);

        public nint GameState { get; private set; }

        public nint IsIngame { get; private set; }

        public nint IsWorldLoaded { get; private set; }

        public nint LastTargetGuid { get; private set; }

        public nint LootWindowOpen { get; private set; }

        public nint MapId { get; private set; }

        public nint NameBase { get; } = new(0x18);

        public nint NameMask { get; } = new(0x24);

        public nint NameStore { get; private set; }

        public nint NameString { get; } = new(0x21);

        public nint NextObject { get; } = new(0x34);

        public nint PartyLeader { get; set; }

        public nint PartyPlayerGuids { get; } = new(0x0);  // unused

        public nint PetGuid { get; private set; }

        public nint PlayerBase { get; private set; }

        public nint PlayerGuid { get; private set; }

        public nint RaidGroupStart { get; } = new(0x0); // unused

        public nint RaidLeader { get; } = new(0x0);  // unused

        public nint RenderFlags { get; } = new(0x0);

        public nint Runes { get; } = new(0x0);

        public nint RuneType { get; } = new(0x0);

        public nint TargetGuid { get; private set; }

        public nint TickCount { get; private set; }

        public nint WowDynobjectPosition { get; } = new(0x1F4);

        public nint WowGameobjectPosition { get; } = new(0x1F4);

        public nint WowObjectDescriptor { get; } = new(0x4);

        public nint WowObjectType { get; } = new(0xC);

        public nint WowPlayerIsSitting { get; } = new(0x3BF8);

        public nint WowUnitCanInterrupt { get; } = new(0xC64);

        public nint WowUnitDbEntry { get; } = new(0x9B4);

        public nint WowUnitDbEntryName { get; } = new(0x6C);

        public nint WowUnitDbEntryType { get; } = new(0x18);

        public nint WowUnitIsAutoAttacking { get; } = new(0x14EC);

        public nint WowUnitPosition { get; } = new(0x838);

        public nint ZoneId { get; private set; }

        public nint ZoneSubText { get; } = new(0x0);

        public nint ZoneText { get; } = new(0x0);

        public void Init(nint mainModuleBase)
        {
            // need to add base to offsets because ASLR is enabled
            BattlegroundFinished = nint.Add(mainModuleBase, 0xDC3050);
            BattlegroundStatus = nint.Add(mainModuleBase, 0xB6335C);
            CameraPointer = nint.Add(mainModuleBase, 0xD64E5C);

            nint clickToMoveBase = nint.Add(mainModuleBase, 0xD0F390);
            ClickToMoveAction = nint.Add(clickToMoveBase, 0x1C);
            ClickToMoveDistance = nint.Add(clickToMoveBase, 0xC);
            ClickToMoveGuid = nint.Add(clickToMoveBase, 0x20);
            ClickToMoveTurnSpeed = nint.Add(clickToMoveBase, 0x4);
            ClickToMoveX = nint.Add(clickToMoveBase, 0x8C);

            ClientConnection = nint.Add(mainModuleBase, 0xEC4628);
            ComboPoints = nint.Add(mainModuleBase, 0xD65BF9);
            CorpsePosition = nint.Add(mainModuleBase, 0xD65ED8);
            EndSceneStaticDevice = nint.Add(mainModuleBase, 0xBB2FB8);
            FunctionGetActivePlayerObject = nint.Add(mainModuleBase, 0x4F84);
            FunctionGetLocalizedText = nint.Add(mainModuleBase, 0x414267);
            FunctionHandleTerrainClick = nint.Add(mainModuleBase, 0x38F129);
            FunctionIsOutdoors = nint.Add(mainModuleBase, 0x4142AC);
            FunctionLuaDoString = nint.Add(mainModuleBase, 0x4FD12);
            FunctionPlayerClickToMove = nint.Add(mainModuleBase, 0x41FB57);
            FunctionSetTarget = nint.Add(mainModuleBase, 0x8CE510);
            FunctionTraceline = nint.Add(mainModuleBase, 0x5EEF7B);
            FunctionUnitOnRightClick = nint.Add(mainModuleBase, 0x8D0268);
            FunctionUnitSetFacing = nint.Add(mainModuleBase, 0x41ADE7);
            FunctionUnitSetFacingSmooth = nint.Add(mainModuleBase, 0x41A41F);
            FunctionUnitGetReaction = nint.Add(mainModuleBase, 0x4153C3);
            GameState = nint.Add(mainModuleBase, 0xD65B16);
            IsIngame = nint.Add(mainModuleBase, 0xB935C0);
            IsWorldLoaded = nint.Add(mainModuleBase, 0xAE1A18);
            LastTargetGuid = nint.Add(mainModuleBase, 0xD65B48);
            LootWindowOpen = nint.Add(mainModuleBase, 0xDD3D44);
            MapId = nint.Add(mainModuleBase, 0xADF5E8);
            NameStore = nint.Add(mainModuleBase, 0xC86840);
            PetGuid = nint.Add(mainModuleBase, 0xDD4A00);
            PlayerBase = nint.Add(mainModuleBase, 0xCFF49C);
            PlayerGuid = nint.Add(mainModuleBase, 0xC95E60);
            PartyLeader = nint.Add(mainModuleBase, 0xDC28EC);
            TargetGuid = nint.Add(mainModuleBase, 0xD65B40);
            TickCount = nint.Add(mainModuleBase, 0xBB2C74);
            ZoneId = nint.Add(mainModuleBase, 0xB595B4);
            // RealmName = 0xEC480E
        }
    }
}