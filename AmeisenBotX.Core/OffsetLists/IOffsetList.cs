using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.OffsetLists
{
    public interface IOffsetList
    {
        IntPtr AccountName { get; }
        IntPtr CharacterSlotSelected { get; }
        IntPtr ChatOpened { get; }
        IntPtr Class { get; }
        IntPtr ClickToMoveAction { get; }
        IntPtr ClickToMoveDistance { get; }
        IntPtr ClickToMoveEnabled { get; }
        IntPtr ClickToMoveGuid { get; }
        IntPtr ClickToMovePointer { get; }
        IntPtr ClickToMoveX { get; }
        IntPtr ClickToMoveY { get; }
        IntPtr ClickToMoveZ { get; }
        IntPtr ClientConnection { get; }
        IntPtr ComboPoints { get; }
        IntPtr ContinentName { get; }
        IntPtr CurrentlyCastingSpellId { get; }
        IntPtr CurrentlyChannelingSpellId { get; }
        IntPtr CurrentObjectManager { get; }
        IntPtr DescriptorEnergy { get; }
        IntPtr DescriptorExp { get; }
        IntPtr DescriptorFactionTemplate { get; }
        IntPtr DescriptorHealth { get; }
        IntPtr DescriptorLevel { get; }
        IntPtr DescriptorMaxEnergy { get; }
        IntPtr DescriptorMaxExp { get; }
        IntPtr DescriptorMaxHealth { get; }
        IntPtr DescriptorTargetGuid { get; }
        IntPtr DescriptorUnitFlags { get; }
        IntPtr DescriptorUnitFlagsDynamic { get; }
        IntPtr EndSceneOffset { get; }
        IntPtr EndSceneOffsetDevice { get; }
        IntPtr EndSceneStaticDevice { get; }
        IntPtr ErrorMessage { get; }
        IntPtr FirstObject { get; }
        IntPtr FunctionGetActivePlayerObject { get; }
        IntPtr FunctionGetLocalizedText { get; }
        IntPtr FunctionGetUnitReaction { get; }
        IntPtr FunctionLuaDoString { get; }
        IntPtr FunctionSendMovementPacket { get; }
        IntPtr FunctionSetTarget { get; }
        IntPtr GameState { get; }
        IntPtr IsWorldLoaded { get; }
        IntPtr LastTargetGuid { get; }
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
        IntPtr PlayerRotation { get; }
        IntPtr Race { get; }
        IntPtr RaidGroupPlayer { get; }
        IntPtr RaidGroupStart { get; }
        IntPtr RaidLeader { get; }
        IntPtr RealmName { get; }
        IntPtr TargetGuid { get; }
        IntPtr TickCount { get; }
        IntPtr WowBuild { get; }
        IntPtr WowObjectDescriptor { get; }
        IntPtr WowObjectGuid { get; }
        IntPtr WowObjectType { get; }
        IntPtr WowUnitPosition { get; }
        IntPtr ZoneId { get; } 
    }
}
