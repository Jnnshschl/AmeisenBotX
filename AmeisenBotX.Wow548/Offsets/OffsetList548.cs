using AmeisenBotX.Wow.Offsets;

namespace AmeisenBotX.Wow548.Offsets
{
    public class OffsetList548 : IOffsetList
    {
        public IntPtr AuraCount1 { get; } = new(0x1218);

        public IntPtr AuraCount2 { get; } = new(0xE14);

        public IntPtr AuraTable1 { get; } = new(0xE18);

        public IntPtr AuraTable2 { get; } = new(0xE1C);

        public IntPtr BattlegroundFinished { get; } = new(0x0);

        public IntPtr BattlegroundStatus { get; } = new(0x0);

        public IntPtr BreathTimer { get; } = new(0x0);

        public IntPtr CameraOffset { get; } = new(0x8208);

        public IntPtr CameraPointer { get; private set; }

        public IntPtr ClickToMoveAction { get; private set; }

        public IntPtr ClickToMoveDistance { get; private set; }

        public IntPtr ClickToMoveEnabled { get; } = new(0x0);

        public IntPtr ClickToMoveGuid { get; private set; }

        public IntPtr ClickToMovePointer { get; } = new(0x0);

        public IntPtr ClickToMoveTurnSpeed { get; private set; }

        public IntPtr ClickToMoveX { get; private set; }

        public IntPtr ClientConnection { get; private set; }

        public IntPtr ComboPoints { get; private set; }

        public IntPtr CorpsePosition { get; private set; }

        public IntPtr CurrentlyCastingSpellId { get; } = new(0xCB8);

        public IntPtr CurrentlyChannelingSpellId { get; } = new(0xCD0);

        public IntPtr CurrentObjectManager { get; } = new(0x462C);

        public IntPtr EndSceneOffset { get; } = new(0xA8);

        public IntPtr EndSceneOffsetDevice { get; } = new(0x2820);

        public IntPtr EndSceneStaticDevice { get; private set; }

        public IntPtr FirstObject { get; } = new(0xCC);

        public IntPtr FunctionGameobjectOnRightClick { get; } = new(0x0);

        public IntPtr FunctionGetActivePlayerObject { get; private set; }

        public IntPtr FunctionGetLocalizedText { get; private set; }

        public IntPtr FunctionHandleTerrainClick { get; private set; }

        public IntPtr FunctionIsOutdoors { get; private set; }

        public IntPtr FunctionLuaDoString { get; private set; }

        public IntPtr FunctionPlayerClickToMove { get; private set; }

        public IntPtr FunctionPlayerClickToMoveStop { get; } = new(0x0);

        public IntPtr FunctionSetTarget { get; private set; }

        public IntPtr FunctionTraceline { get; private set; }

        public IntPtr FunctionUnitGetReaction { get; private set; }

        public IntPtr FunctionUnitOnRightClick { get; private set; }

        public IntPtr FunctionUnitSetFacing { get; private set; }

        public IntPtr FunctionWorldFrame { get; } = new(0x0);

        public IntPtr FunctionWorldRender { get; } = new(0x0);

        public IntPtr FunctionWorldRenderWorld { get; } = new(0x0);

        public IntPtr GameState { get; private set; }

        public IntPtr IsIngame { get; private set; }

        public IntPtr IsWorldLoaded { get; private set; }

        public IntPtr LastTargetGuid { get; private set; }

        public IntPtr LootWindowOpen { get; private set; }

        public IntPtr MapId { get; private set; }

        public IntPtr NameBase { get; } = new(0x18);

        public IntPtr NameMask { get; } = new(0x24);

        public IntPtr NameStore { get; private set; }

        public IntPtr NameString { get; } = new(0x21);

        public IntPtr NextObject { get; } = new(0x34);

        public IntPtr PartyLeader { get; } = new(0x0);

        public IntPtr PartyPlayerGuids { get; } = new(0x0);

        public IntPtr PetGuid { get; private set; }

        public IntPtr PlayerBase { get; private set; }

        public IntPtr PlayerGuid { get; private set; }

        public IntPtr RaidGroupStart { get; } = new(0x0);

        public IntPtr RaidLeader { get; } = new(0x0);

        public IntPtr RenderFlags { get; } = new(0x0);

        public IntPtr Runes { get; } = new(0x0);

        public IntPtr RuneType { get; } = new(0x0);

        public IntPtr TargetGuid { get; private set; }

        public IntPtr TickCount { get; private set; }

        public IntPtr WowDynobjectPosition { get; } = new(0x0);

        public IntPtr WowGameobjectPosition { get; } = new(0x1F4);

        public IntPtr WowObjectDescriptor { get; } = new(0x4);

        public IntPtr WowObjectType { get; } = new(0xC);

        public IntPtr WowUnitFlyFlags { get; } = new(0x0);

        public IntPtr WowUnitFlyFlagsPointer { get; } = new(0x0);

        public IntPtr WowUnitIsAutoAttacking { get; } = new(0x0);

        public IntPtr WowUnitName1 { get; } = new(0x6C);

        public IntPtr WowUnitName2 { get; } = new(0x0);

        public IntPtr WOwUnitCanInterrupt { get; } = new(0xC64);

        public IntPtr WowUnitPosition { get; } = new(0x838);

        public IntPtr WowUnitSwimFlags { get; } = new(0x0);

        public IntPtr ZoneId { get; private set; }

        public IntPtr ZoneSubText { get; } = new(0x0);

        public IntPtr ZoneText { get; } = new(0x0);

        public IntPtr CollisionM2C { get; } = new(0x0);

        public IntPtr CollisionM2S { get; } = new(0x0);

        public IntPtr CollisionWMO { get; } = new(0x0);

        public IntPtr ClimbAngle { get; } = new(0x0);

        public IntPtr StaticPlayer { get; } = new(0x0);

        public void Init(IntPtr mainModuleBase)
        {

            // need to add base to offsets because ASLR is enabled
            CameraPointer = IntPtr.Add(mainModuleBase, 0xD64E5C);

            IntPtr clickToMoveBase = IntPtr.Add(mainModuleBase, 0xD0F390);
            ClickToMoveAction = IntPtr.Add(clickToMoveBase, 0x1C);
            ClickToMoveDistance = IntPtr.Add(clickToMoveBase, 0xC);
            ClickToMoveGuid = IntPtr.Add(clickToMoveBase, 0x20);
            ClickToMoveTurnSpeed = IntPtr.Add(clickToMoveBase, 0x4);
            ClickToMoveX = IntPtr.Add(clickToMoveBase, 0x8C);

            ClientConnection = IntPtr.Add(mainModuleBase, 0xEC4628);
            ComboPoints = IntPtr.Add(mainModuleBase, 0xD65BF9);
            CorpsePosition = IntPtr.Add(mainModuleBase, 0xD65ED8);
            EndSceneStaticDevice = IntPtr.Add(mainModuleBase, 0xBB2FB8);
            FunctionGetActivePlayerObject = IntPtr.Add(mainModuleBase, 0x4F84);
            FunctionGetLocalizedText = IntPtr.Add(mainModuleBase, 0x414267);
            FunctionHandleTerrainClick = IntPtr.Add(mainModuleBase, 0x38F129);
            FunctionIsOutdoors = IntPtr.Add(mainModuleBase, 0x414B53);
            FunctionLuaDoString = IntPtr.Add(mainModuleBase, 0x4FD12);
            FunctionPlayerClickToMove = IntPtr.Add(mainModuleBase, 0x41FB57);
            FunctionSetTarget = IntPtr.Add(mainModuleBase, 0x8CE880);
            FunctionTraceline = IntPtr.Add(mainModuleBase, 0x5EEF7B);
            FunctionUnitOnRightClick = IntPtr.Add(mainModuleBase, 0x8D0268);
            FunctionUnitSetFacing = IntPtr.Add(mainModuleBase, 0x8A9F78);
            FunctionUnitGetReaction = IntPtr.Add(mainModuleBase, 0x4A799C);
            GameState = IntPtr.Add(mainModuleBase, 0xD65B16);
            IsIngame = IntPtr.Add(mainModuleBase, 0xB935C0);
            IsWorldLoaded = IntPtr.Add(mainModuleBase, 0xAE1A18);
            LastTargetGuid = IntPtr.Add(mainModuleBase, 0xD65B48);
            LootWindowOpen = IntPtr.Add(mainModuleBase, 0xDD3D44);
            MapId = IntPtr.Add(mainModuleBase, 0xADF5E8);
            NameStore = IntPtr.Add(mainModuleBase, 0xC86840);
            PetGuid = IntPtr.Add(mainModuleBase, 0xDD4A00);
            PlayerBase = IntPtr.Add(mainModuleBase, 0xCFF49C);
            PlayerGuid = IntPtr.Add(mainModuleBase, 0xC95E60);
            TargetGuid = IntPtr.Add(mainModuleBase, 0xD65B40);
            TickCount = IntPtr.Add(mainModuleBase, 0xBB2C74);
            ZoneId = IntPtr.Add(mainModuleBase, 0xB595B4);
            // RealmName = 0xEC480E
        }
    }
}