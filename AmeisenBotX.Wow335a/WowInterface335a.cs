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
    /// <summary>
    /// WowInterface for the game version 3.3.5a 12340.
    /// </summary>
    public class WowInterface335a : IWowInterface
    {
        public WowInterface335a(IMemoryApi memoryApi, IOffsetList offsetList, List<IHookModule> hookModules)
        {
            HookModules = hookModules;

            ObjectManager = new(memoryApi, offsetList);
            Hook = new(memoryApi, offsetList, ObjectManager);
            Hook.OnGameInfoPush += ObjectManager.HookManagerOnGameInfoPush;
        }

        public ulong HookCallCount => Hook.HookCallCount;

        public bool IsReady => Hook.IsWoWHooked;

        public IObjectProvider Objects => ObjectManager;

        public WowUnit Player => ObjectManager.Player;

        private EndSceneHook Hook { get; }

        private List<IHookModule> HookModules { get; }

        private ObjectManager ObjectManager { get; }

        public void Dispose()
        {
            Hook.Unhook();
        }

        public void LuaAbandonQuestsNotIn(IEnumerable<string> quests) => Hook.LuaAbandonQuestsNotIn(quests);

        public void LuaAcceptBattlegroundInvite() => Hook.LuaAcceptBattlegroundInvite();

        public void LuaAcceptPartyInvite() => Hook.LuaAcceptPartyInvite();

        public void LuaAcceptQuest() => Hook.LuaAcceptQuest();

        public void LuaAcceptQuests() => Hook.LuaAcceptQuests();

        public void LuaAcceptResurrect() => Hook.LuaAcceptResurrect();

        public void LuaAcceptSummon() => Hook.LuaAcceptSummon();

        public bool LuaAutoLootEnabled() => Hook.LuaAutoLootEnabled();

        public void LuaCallCompanion(int index) => Hook.LuaCallCompanion(index);

        public void LuaCastSpell(string spellName, bool castOnSelf = false) => Hook.LuaCastSpell(spellName, castOnSelf);

        public void LuaCastSpellById(int spellId) => Hook.LuaCastSpellById(spellId);

        public void LuaClickUiElement(string uiElement) => Hook.LuaClickUiElement(uiElement);

        public void LuaCofirmLootRoll() => Hook.LuaCofirmLootRoll();

        public void LuaCofirmReadyCheck(bool isReady) => Hook.LuaCofirmReadyCheck(isReady);

        public void LuaCofirmStaticPopup() => Hook.LuaCofirmStaticPopup();

        public void LuaCompleteQuest() => Hook.LuaCompleteQuest();

        public void LuaDeleteInventoryItemByName(string name) => Hook.LuaDeleteInventoryItemByName(name);

        public void LuaDismissCompanion() => Hook.LuaDismissCompanion();

        public bool LuaDoString(string v) => Hook.LuaDoString(v);

        public void LuaEquipItem(string itemName, int slot = -1) => Hook.LuaEquipItem(itemName, slot);

        public IEnumerable<int> LuaGetCompletedQuests() => Hook.LuaGetCompletedQuests();

        public string LuaGetEquipmentItems() => Hook.LuaGetEquipmentItems();

        public int LuaGetFreeBagSlotCount() => Hook.LuaGetFreeBagSlotCount();

        public string[] LuaGetGossipTypes() => Hook.LuaGetGossipTypes();

        public string LuaGetInventoryItems() => Hook.LuaGetInventoryItems();

        public string LuaGetItemJsonByNameOrLink(string itemLink) => Hook.LuaGetItemJsonByNameOrLink(itemLink);

        public string LuaGetLootRollItemLink(int rollId) => Hook.LuaGetLootRollItemLink(rollId);

        public string LuaGetMoney() => Hook.LuaGetMoney();

        public string LuaGetMounts() => Hook.LuaGetMounts();

        public bool LuaGetNumQuestLogChoices(out int numChoices) => Hook.LuaGetNumQuestLogChoices(out numChoices);

        public bool LuaGetQuestLogChoiceItemLink(int i, out string itemLink) => Hook.LuaGetQuestLogChoiceItemLink(i, out itemLink);

        public bool LuaGetQuestLogIdByTitle(string name, out int questLogId) => Hook.LuaGetQuestLogIdByTitle(name, out questLogId);

        public void LuaGetQuestReward(int i) => Hook.LuaGetQuestReward(i);

        public Dictionary<string, (int, int)> LuaGetSkills() => Hook.LuaGetSkills();

        public double LuaGetSpellCooldown(string spellName) => Hook.LuaGetSpellCooldown(spellName);

        public string LuaGetSpellNameById(int spellId) => Hook.LuaGetSpellNameById(spellId);

        public string LuaGetSpells() => Hook.LuaGetSpells();

        public string LuaGetTalents() => Hook.LuaGetTalents();

        public (string, int) LuaGetUnitCastingInfo(WowLuaUnit target) => Hook.LuaGetUnitCastingInfo(target.ToString());

        public int LuaGetUnspentTalentPoints() => Hook.LuaGetUnspentTalentPoints();

        public bool LuaIsInLfgGroup() => Hook.LuaIsInLfgGroup();

        public void LuaLeaveBattleground() => Hook.LuaLeaveBattleground();

        public void LuaLootEveryThing() => Hook.LuaLootEveryThing();

        public void LuaLootMoneyAndQuestItems() => Hook.LuaLootMoneyAndQuestItems();

        public void LuaQueryQuestsCompleted() => Hook.LuaQueryQuestsCompleted();

        public void LuaRepairAllItems() => Hook.LuaRepairAllItems();

        public void LuaRepopMe() => Hook.LuaRepopMe();

        public void LuaRetrieveCorpse() => Hook.LuaRetrieveCorpse();

        public void LuaRollOnLoot(int rollId, WowRollType rollType) => Hook.LuaRollOnLoot(rollId, (int)rollType);

        public void LuaSelectGossipOption(int gossipId) => Hook.LuaSelectGossipOption(gossipId);

        public void LuaSelectQuestByNameOrGossipId(string name, int gossipId, bool isAvailableQuest) => Hook.LuaSelectQuestByNameOrGossipId(name, gossipId, isAvailableQuest);

        public void LuaSelectQuestLogEntry(int questLogId) => Hook.LuaSelectQuestLogEntry(questLogId);

        public void LuaSendChatMessage(string msg) => Hook.LuaSendChatMessage(msg);

        public void LuaSetLfgRole(WowRole wowRole) => Hook.LuaSetLfgRole((int)wowRole);

        public void LuaSpellStopCasting() => Hook.LuaSpellStopCasting();

        public void LuaStartAutoAttack() => Hook.LuaStartAutoAttack();

        public bool LuaUiIsVisible(params string[] v) => Hook.LuaUiIsVisible(v);

        public void LuaUseContainerItem(int bagId, int bagSlot) => Hook.LuaUseContainerItem(bagId, bagSlot);

        public void LuaUseInventoryItem(WowEquipmentSlot equipmentSlot) => Hook.LuaUseInventoryItem((int)equipmentSlot);

        public void LuaUseItemByName(string name) => Hook.LuaUseItemByName(name);

        public bool Setup()
        {
            return Hook.Hook(7, HookModules);
        }

        public void SetWorldLoadedCheck(bool enabled) => Hook.BotOverrideWorldLoadedCheck(enabled);

        public void Tick()
        {
            if (ObjectManager.RefreshIsWorldLoaded())
            {
                ObjectManager.UpdateWowObjects();
            }
        }

        public void WowClearTarget() => Hook.WowClearTarget();

        public void WowClickOnTerrain(Vector3 position) => Hook.WowClickOnTerrain(position);

        public void WowEnableClickToMove() => Hook.WowEnableClickToMove();

        public bool WowExecuteLuaAndRead((string, string) p, out string result) => Hook.WowExecuteLuaAndRead(p, out result);

        public void WowFacePosition(IntPtr playerBase, Vector3 playerPosition, Vector3 position) => Hook.WowFacePosition(playerBase, playerPosition, position);

        public WowUnitReaction WowGetReaction(IntPtr a, IntPtr b) => (WowUnitReaction)Hook.WowGetUnitReaction(a, b);

        public Dictionary<int, int> WowGetRunesReady() => Hook.WowGetRunesReady();

        public bool WowIsClickToMoveActive() => Hook.WowIsClickToMoveActive();

        public bool WowIsInLineOfSight(Vector3 position1, Vector3 position2, float heightAdjust = 1.5F) => Hook.WowIsInLineOfSight(position1, position2, heightAdjust);

        public bool WowIsRuneReady(int id) => Hook.WowIsRuneReady(id);

        public void WowObjectRightClick(IntPtr objectBase) => Hook.WowObjectRightClick(objectBase);

        public void WowSetFacing(IntPtr playerBase, float angle) => Hook.WowSetFacing(playerBase, angle);

        public void WowSetRenderState(bool state) => Hook.WowSetRenderState(state);

        public void WowStopClickToMove(IntPtr playerBase) => Hook.WowStopClickToMove(playerBase);

        public void WowStopClickToMove() => Hook.WowStopClickToMove(Player.BaseAddress);

        public void WowTargetGuid(ulong guid) => Hook.WowTargetGuid(guid);

        public void WowUnitRightClick(IntPtr unitBase) => Hook.WowUnitRightClick(unitBase);
    }
}