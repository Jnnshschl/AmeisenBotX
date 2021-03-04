using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Data.Objects.Raw;
using AmeisenBotX.Core.Hook.Modules;
using AmeisenBotX.Core.Hook.Structs;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Hook
{
    /// <summary>
    /// This is the main interactionpoint with the wow game it
    /// handles function calling, execution of asm code and
    /// hooking the EndScene.
    /// </summary>
    public interface IHookManager
    {
        /// <summary>
        /// This event will to push the lastest GameInfo struct.
        /// </summary>
        event Action<GameInfo> OnGameInfoPush;

        /// <summary>
        /// Returns the amount of hook calls since the last time
        /// this variable was retrieved.
        /// </summary>
        ulong HookCallCount { get; }

        /// <summary>
        /// Reads the first byte of the EndScene hook, this
        /// should be a JMP instruction if wow is hooked.
        /// </summary>
        bool IsWoWHooked { get; }

        void BotOverrideWorldLoadedCheck(bool status);

        bool Hook(int hookSize, List<IHookModule> hookModules);

        void LuaAbandonQuestsNotIn(IEnumerable<string> questNames);

        void LuaAcceptBattlegroundInvite();

        void LuaAcceptPartyInvite();

        void LuaAcceptQuest();

        void LuaAcceptQuests();

        void LuaAcceptResurrect();

        void LuaAcceptSummon();

        bool LuaAutoLootEnabled();

        void LuaCallCompanion(int index, string type = "MOUNT");

        void LuaCancelSummon();

        void LuaCastSpell(string name, bool castOnSelf = false);

        void LuaCastSpellById(int spellId);

        void LuaClickUiElement(string elementName);

        void LuaCofirmLootRoll();

        void LuaCofirmReadyCheck(bool isReady);

        void LuaCofirmStaticPopup();

        void LuaCompleteQuest();

        void LuaCompleteQuestAndGetReward(int questlogId, int rewardId, int gossipId);

        void LuaDeclinePartyInvite();

        void LuaDeclineResurrect();

        void LuaDeleteInventoryItemByName(string itemName);

        void LuaDismissCompanion(string type = "MOUNT");

        bool LuaDoString(string command);

        void LuaEquipItem(IWowItem newItem, IWowItem currentItem = null);

        IEnumerable<int> LuaGetCompletedQuests();

        string LuaGetEquipmentItems();

        int LuaGetFreeBagSlotCount();

        bool LuaGetGossipActiveQuestTitleById(int gossipId, out string title);

        bool LuaGetGossipIdByActiveQuestTitle(string title, out int gossipId);

        bool LuaGetGossipIdByAvailableQuestTitle(string title, out int gossipId);

        int LuaGetGossipOptionCount();

        string[] LuaGetGossipTypes();

        string LuaGetInventoryItems();

        string LuaGetItemBySlot(int itemslot);

        string LuaGetItemJsonByNameOrLink(string itemName);

        string LuaGetItemStats(string itemLink);

        string LuaGetLootRollItemLink(int rollId);

        string LuaGetMoney();

        string LuaGetMounts();

        bool LuaGetNumQuestLogChoices(out int numChoices);

        bool LuaGetQuestLogChoiceItemLink(int index, out string itemLink);

        bool LuaGetQuestLogIdByTitle(string title, out int questLogId);

        void LuaGetQuestReward(int id);

        Dictionary<string, (int, int)> LuaGetSkills();

        /// <summary>
        /// Get the spells cooldown left in milliseconds
        /// </summary>
        /// <param name="spellName"></param>
        /// <returns>ms cooldown left</returns>
        int LuaGetSpellCooldown(string spellName);

        string LuaGetSpellNameById(int spellId);

        string LuaGetSpells();

        string LuaGetTalents();

        (string, int) LuaGetUnitCastingInfo(WowLuaUnit luaunit);

        int LuaGetUnspentTalentPoints();

        bool LuaHasUnitStealableBuffs(WowLuaUnit luaUnit);

        bool LuaIsBgInviteReady();

        bool LuaIsGhost(WowLuaUnit luaUnit);

        bool LuaIsInLfgGroup();

        bool LuaIsOutdoors();

        void LuaKickNpcsOutOfVehicle();

        void LuaLearnAllAvaiableSpells();

        void LuaLeaveBattleground();

        void LuaLootEveryThing();

        void LuaLootMoneyAndQuestItems();

        void LuaQueryQuestsCompleted();

        void LuaQueueBattlegroundByName(string bgName);

        void LuaRepairAllItems();

        void LuaRepopMe();

        void LuaRetrieveCorpse();

        void LuaRollOnLoot(int rollId, WowRollType rollType);

        void LuaSelectGossipActiveQuest(int gossipId);

        void LuaSelectGossipAvailableQuest(int gossipId);

        void LuaSelectGossipOption(int gossipId);

        void LuaSelectQuestByNameOrGossipId(string questName, int gossipId, bool isAvailableQuest);

        void LuaSelectQuestLogEntry(int gossipId);

        void LuaSellAllItems();

        void LuaSellItemsByName(string itemName);

        void LuaSellItemsByQuality(WowItemQuality itemQuality);

        void LuaSendChatMessage(string message);

        void LuaSendItemMailToCharacter(string itemName, string receiver);

        void LuaSetLfgRole(WowRole combatClassRole);

        void LuaSpellStopCasting();

        void LuaStartAutoAttack();

        void LuaTargetUnit(WowLuaUnit unit);

        bool LuaUiIsVisible(params string[] uiElement);

        void LuaUseContainerItem(int bagId, int bagSlot);

        void LuaUseInventoryItem(WowEquipmentSlot equipmentSlot);

        void LuaUseItemByName(string itemName);

        void Unhook();

        void WowClearTarget();

        void WowClickOnTerrain(Vector3 position);

        void WowClickToMove(WowPlayer player, Vector3 position);

        void WowEnableClickToMove();

        bool WowExecuteLuaAndRead((string, string) commandVariableTuple, out string result);

        bool WowExecuteLuaAndRead(string command, string variable, out string result);

        void WowFacePosition(WowPlayer player, Vector3 positionToFace);

        bool WowGetLocalizedText(string variable, out string result);

        Dictionary<WowRuneType, int> WowGetRunesReady();

        IEnumerable<WowAura> WowGetUnitAuras(IntPtr baseAddress, out int auraCount);

        WowUnitReaction WowGetUnitReaction(WowUnit wowUnitA, WowUnit wowUnitB);

        bool WowIsClickToMoveActive();

        bool WowIsInLineOfSight(Vector3 start, Vector3 end, float heightAdjust = 1.5f);

        bool WowIsRuneReady(int runeId);

        void WowObjectRightClick(WowObject gObject);

        void WowSetFacing(WowUnit unit, float angle);

        void WowSetRenderState(bool renderingEnabled);

        void WowStopClickToMove();

        void WowTargetGuid(ulong guid);

        byte WowTraceLine(Vector3 start, Vector3 end, out Vector3 result, uint flags = 0x120171);

        void WowUnitRightClick(WowUnit wowUnit);
    }
}