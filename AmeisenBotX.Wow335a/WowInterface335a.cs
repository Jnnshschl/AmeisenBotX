using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Hook.Modules;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Hook;
using AmeisenBotX.Wow335a.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow335a
{
    public class WowInterface335a : INewWowInterface
    {
        public WowInterface335a(XMemory xMemory, IOffsetList offsetList, List<IHookModule> hookModules)
        {
            HookModules = hookModules;

            Hook = new(xMemory, offsetList);
            ObjectManager = new(xMemory, offsetList);
        }

        public WowUnit Player => ObjectManager.Player;

        public IObjectProvider Objects => ObjectManager;

        private EndSceneHook Hook { get; }

        private List<IHookModule> HookModules { get; }

        private ObjectManager ObjectManager { get; }

        public ulong HookCallCount => throw new NotImplementedException();

        public bool IsWoWHooked => throw new NotImplementedException();

        public bool Dispose()
        {
            return true;
        }

        public WowUnitReaction GetReaction(IntPtr a, IntPtr b) => (WowUnitReaction)Hook.WowGetUnitReaction(a, b);

        public bool Setup()
        {
            return Hook.Hook(7, HookModules);
        }

        public void Tick()
        {
            ObjectManager.UpdateWowObjects();
        }

        public void LuaCastSpell(string spellName, bool castOnSelf = false) => Hook.LuaCastSpell(spellName, castOnSelf);

        public void WowStopClickToMove(IntPtr playerBase) => Hook.WowStopClickToMove(playerBase);

        public void LuaCastSpellById(int spellId) => Hook.LuaCastSpellById(spellId);

        public bool WowIsClickToMoveActive() => Hook.WowIsClickToMoveActive();

        public void WowClearTarget() => Hook.WowClearTarget();

        public void WowTargetGuid(ulong guid) => Hook.WowTargetGuid(guid);

        public bool WowIsInLineOfSight(Vector3 position1, Vector3 position2, float heightAdjust = 1.5F) => Hook.WowIsInLineOfSight(position1, position2, heightAdjust);

        public void LuaStartAutoAttack() => Hook.LuaStartAutoAttack();

        public void LuaSendChatMessage(string msg) => Hook.LuaSendChatMessage(msg);

        public void WowStopClickToMove() => Hook.WowStopClickToMove(Player.BaseAddress);

        public void WowUnitRightClick(IntPtr unitBase) => Hook.WowUnitRightClick(unitBase);

        public bool LuaDoString(string v) => Hook.LuaDoString(v);

        public void LuaAbandonQuestsNotIn(IEnumerable<string> quests) => Hook.LuaAbandonQuestsNotIn(quests);

        public void LuaDeleteInventoryItemByName(string name) => Hook.LuaDeleteInventoryItemByName(name);

        public void LuaQueryQuestsCompleted() => Hook.LuaQueryQuestsCompleted();

        public IEnumerable<int> LuaGetCompletedQuests() => Hook.LuaGetCompletedQuests();

        public string LuaGetTalents() => Hook.LuaGetTalents();

        public void LuaRepopMe() => Hook.LuaRepopMe();

        public void LuaCofirmStaticPopup() => Hook.LuaCofirmStaticPopup();

        public void LuaClickUiElement(string uiElement) => Hook.LuaClickUiElement(uiElement);

        public void LuaSetLfgRole(WowRole wowRole) => Hook.LuaSetLfgRole((int)wowRole);

        public string LuaGetLootRollItemLink(int rollId) => Hook.LuaGetLootRollItemLink(rollId);

        public int LuaGetFreeBagSlotCount() => Hook.LuaGetFreeBagSlotCount();

        public string LuaGetItemJsonByNameOrLink(string itemLink) => Hook.LuaGetItemJsonByNameOrLink(itemLink);

        public void LuaRollOnLoot(int rollId, WowRollType rollType) => Hook.LuaRollOnLoot(rollId, (int)rollType);

        public string LuaGetInventoryItems() => Hook.LuaGetInventoryItems();

        public void LuaCofirmLootRoll() => Hook.LuaCofirmLootRoll();

        public void LuaLootMoneyAndQuestItems() => Hook.LuaLootMoneyAndQuestItems();

        public void LuaLootEveryThing() => Hook.LuaLootEveryThing();

        public void LuaAcceptPartyInvite() => Hook.LuaAcceptPartyInvite();

        public void LuaAcceptBattlegroundInvite() => Hook.LuaAcceptBattlegroundInvite();

        public void LuaAcceptQuests() => Hook.LuaAcceptQuests();

        public void LuaCofirmReadyCheck(bool isReady) => Hook.LuaCofirmReadyCheck(isReady);

        public void LuaAcceptResurrect() => Hook.LuaAcceptResurrect();

        public void LuaAcceptSummon() => Hook.LuaAcceptSummon();

        public int LuaGetUnspentTalentPoints() => Hook.LuaGetUnspentTalentPoints();

        public void WowEnableClickToMove() => Hook.WowEnableClickToMove();

        public void LuaCallCompanion(int index) => Hook.LuaCallCompanion(index);

        public void LuaDismissCompanion() => Hook.LuaDismissCompanion();

        public void LuaEquipItem(string itemName, int slot = -1) => Hook.LuaEquipItem(itemName, slot);

        public Dictionary<string, (int, int)> LuaGetSkills() => Hook.LuaGetSkills();

        public string LuaGetMounts() => Hook.LuaGetMounts();

        public ReadOnlySpan<char> LuaGetMoney() => Hook.LuaGetMoney();

        public void LuaAcceptQuest() => Hook.LuaAcceptQuest();

        public void LuaSelectQuestByNameOrGossipId(string name, int gossipId, bool isAvailableQuest) => Hook.LuaSelectQuestByNameOrGossipId(name, gossipId, isAvailableQuest);

        public bool LuaGetQuestLogIdByTitle(string name, out int questLogId) => Hook.LuaGetQuestLogIdByTitle(name, out questLogId);

        public void LuaCompleteQuest() => Hook.LuaCompleteQuest();

        public void LuaSelectQuestLogEntry(int questLogId) => Hook.LuaSelectQuestLogEntry(questLogId);

        public bool LuaGetNumQuestLogChoices(out int numChoices) => Hook.LuaGetNumQuestLogChoices(out numChoices);

        public bool LuaGetQuestLogChoiceItemLink(int i, out string itemLink) => Hook.LuaGetQuestLogChoiceItemLink(i, out itemLink);

        public void LuaGetQuestReward(int i) => Hook.LuaGetQuestReward(i);

        public void WowObjectRightClick(IntPtr objectBase) => Hook.WowObjectRightClick(objectBase);

        public Dictionary<int, int> WowGetRunesReady() => Hook.WowGetRunesReady();

        public void WowFacePosition(IntPtr playerBase, Vector3 playerPosition, Vector3 position) => Hook.WowFacePosition(playerBase, playerPosition, position);

        public bool WowExecuteLuaAndRead((string, string) p, out string result) => Hook.WowExecuteLuaAndRead(p, out result);

        public void WowClickOnTerrain(Vector3 position) => Hook.WowClickOnTerrain(position);

        public void LuaUseItemByName(string name) => Hook.LuaUseItemByName(name);

        public void LuaSpellStopCasting() => Hook.LuaSpellStopCasting();

        public bool LuaAutoLootEnabled() => Hook.LuaAutoLootEnabled();

        public double LuaGetSpellCooldown(string spellName) => Hook.LuaGetSpellCooldown(spellName);

        public void WowSetRenderState(bool state) => Hook.WowSetRenderState(state);

        public string LuaGetEquipmentItems() => Hook.LuaGetEquipmentItems();

        public string LuaGetSpellNameById(int spellId) => Hook.LuaGetSpellNameById(spellId);

        public string LuaGetSpells() => Hook.LuaGetSpells();

        public (string, int) LuaGetUnitCastingInfo(WowLuaUnit target) => Hook.LuaGetUnitCastingInfo(target.ToString());

        public bool LuaIsInLfgGroup() => Hook.LuaIsInLfgGroup();

        public void LuaLeaveBattleground() => Hook.LuaLeaveBattleground();

        public void LuaRepairAllItems() => Hook.LuaRepairAllItems();

        public void LuaRetrieveCorpse() => Hook.LuaRetrieveCorpse();

        public void LuaSelectGossipOption(int gossipId) => Hook.LuaSelectGossipOption(gossipId);

        public void LuaUseContainerItem(int bagId, int bagSlot) => Hook.LuaUseContainerItem(bagId, bagSlot);

        public void LuaUseInventoryItem(WowEquipmentSlot equipmentSlot) => Hook.LuaUseInventoryItem((int)equipmentSlot);

        public bool WowIsRuneReady(int id) => Hook.WowIsRuneReady(id);

        public void WowSetFacing(IntPtr playerBase, float angle) => Hook.WowSetFacing(playerBase, angle);

        public void BotOverrideWorldLoadedCheck(bool enabled) => Hook.BotOverrideWorldLoadedCheck(enabled);

        public bool LuaUiIsVisible(params string[] v) => Hook.LuaUiIsVisible(v);

        public string[] LuaGetGossipTypes() => Hook.LuaGetGossipTypes();
    }
}