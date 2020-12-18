using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Structs;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Hook
{
    public interface IHookManager
    {
        ulong HookCallCount { get; }

        bool IsWoWHooked { get; }

        void BotOverrideWorldLoadedCheck(bool status);

        void DisposeHook();

        void LuaAcceptBattlegroundInvite();

        void LuaAcceptPartyInvite();

        void LuaSelectGossipActiveQuest(int gossipId);

        void LuaCompleteQuest();

        void LuaAcceptQuest();

        void LuaAcceptQuests();

        void LuaAcceptResurrect();

        void LuaAcceptSummon();

        void LuaCallCompanion(int index, string type = "MOUNT");

        void LuaCancelSummon();

        void LuaCastSpell(string name, bool castOnSelf = false);

        void LuaCastSpellById(int spellId);

        void LuaClickUiElement(string elementName);

        void LuaCofirmLootRoll();

        void LuaCofirmReadyCheck(bool isReady);

        void LuaCofirmStaticPopup();

        void LuaCompleteQuestAndGetReward(int questlogId, int rewardId, int gossipId);

        void LuaDeclinePartyInvite();

        void LuaDeclineResurrect();

        void LuaDismissCompanion(string type = "MOUNT");

        bool LuaDoString(string command);

        void LuaEquipItem(IWowItem newItem, IWowItem currentItem = null);

        List<int> LuaGetCompletedQuests();

        string LuaGetEquipmentItems();

        int LuaGetFreeBagSlotCount();

        int LuaGetGossipOptionCount();

        string[] LuaGetGossipTypes();

        string LuaGetInventoryItems();

        string LuaGetItemBySlot(int itemslot);

        string LuaGetItemJsonByNameOrLink(string itemName);

        string LuaGetItemStats(string itemLink);

        string LuaGetLootRollItemLink(int rollId);

        string LuaGetMoney();

        string LuaGetMounts();

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

        void LuaRollOnLoot(int rollId, RollType rollType);

        void LuaSelectGossipOption(int gossipId);

        void LuaSellAllItems();

        void LuaSellItemsByName(string itemName);

        void LuaSellItemsByQuality(ItemQuality itemQuality);

        void LuaSendChatMessage(string message);

        void LuaSendItemMailToCharacter(string itemName, string receiver);

        void LuaSetLfgRole(CombatClassRole combatClassRole);

        void LuaSpellStopCasting();

        void LuaStartAutoAttack();

        void LuaTargetUnit(WowLuaUnit unit);

        bool LuaUiIsVisible(params string[] uiElement);

        bool LuaAutoLootEnabled();

        void LuaUseContainerItem(int bagId, int bagSlot);

        void LuaUseInventoryItem(EquipmentSlot equipmentSlot);

        void LuaUseItemByName(string itemName);

        void LuaAbandonQuestsNotIn(IEnumerable<string> questNames);

        bool LuaGetGossipIdByTitle(string title, out int gossipId);

        bool LuaQuestLogIdByTitle(string title, out int questLogId);

        void WowClearTarget();

        void WowClickOnTerrain(Vector3 position);

        void WowClickToMove(WowPlayer player, Vector3 position);

        void WowEnableClickToMove();

        bool WowExecuteLuaAndRead((string, string) commandVariableTuple, out string result);

        bool WowExecuteLuaAndRead(string command, string variable, out string result);

        void WowFacePosition(WowPlayer player, Vector3 positionToFace);

        bool WowGetLocalizedText(string variable, out string result);

        Dictionary<RuneType, int> WowGetRunesReady();

        public void WowGetUnitAuras(IntPtr baseAddress, ref WowAura[] auraTable, out int auraCount);

        WowUnitReaction WowGetUnitReaction(WowUnit wowUnitA, WowUnit wowUnitB);

        bool WowIsClickToMoveActive();

        bool WowIsInLineOfSight(Vector3 start, Vector3 end, float heightAdjust = 1.5f);

        bool WowIsRuneReady(int runeId);

        void WowObjectRightClick(WowObject gObject);

        void WowSetFacing(WowUnit unit, float angle);

        void WowSetRenderState(bool renderingEnabled);

        bool WowSetupEndsceneHook();

        void WowStopClickToMove();

        void WowTargetGuid(ulong guid);

        byte WowTraceLine(Vector3 start, Vector3 end, out Vector3 result, uint flags = 0x120171);

        void WowUnitRightClick(WowUnit wowUnit);
    }
}