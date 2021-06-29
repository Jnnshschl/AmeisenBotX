using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow
{
    /// <summary>
    /// Interface to the wow game. All functions that interact with the game should be reachable via this interface.
    /// </summary>
    public interface INewWowInterface
    {
        ulong HookCallCount { get; }

        IObjectProvider Objects { get; }

        WowUnit Player { get; }
        bool IsWoWHooked { get; }

        bool Dispose();

        WowUnitReaction GetReaction(IntPtr a, IntPtr b);

        void LuaAbandonQuestsNotIn(IEnumerable<string> enumerable);

        void LuaAcceptBattlegroundInvite();

        void LuaAcceptPartyInvite();

        void LuaAcceptQuest();

        void LuaAcceptQuests();

        void LuaAcceptResurrect();

        void LuaAcceptSummon();

        bool LuaAutoLootEnabled();
        bool LuaUiIsVisible(params string[] v);
        void LuaCallCompanion(int index);

        void LuaCastSpell(string spellName, bool castOnSelf = false);
        string[] LuaGetGossipTypes();
        void LuaCastSpellById(int spellId);

        void LuaClickUiElement(string v);

        void LuaCofirmLootRoll();

        void LuaCofirmReadyCheck(bool v);

        void LuaCofirmStaticPopup();

        void LuaCompleteQuest();

        void LuaDeleteInventoryItemByName(string name);

        void LuaDismissCompanion();

        bool LuaDoString(string v);

        void LuaEquipItem(string itemName, int slot = -1);

        IEnumerable<int> LuaGetCompletedQuests();

        string LuaGetEquipmentItems();

        int LuaGetFreeBagSlotCount();

        string LuaGetInventoryItems();

        string LuaGetItemJsonByNameOrLink(string itemLink);

        string LuaGetLootRollItemLink(int rollId);

        ReadOnlySpan<char> LuaGetMoney();

        string LuaGetMounts();

        bool LuaGetNumQuestLogChoices(out int numChoices);

        bool LuaGetQuestLogChoiceItemLink(int i, out string itemLink);

        bool LuaGetQuestLogIdByTitle(string name, out int questLogId);

        void LuaGetQuestReward(int i);

        Dictionary<string, (int, int)> LuaGetSkills();

        double LuaGetSpellCooldown(string spellName);

        string LuaGetSpellNameById(int spellId);

        string LuaGetSpells();

        string LuaGetTalents();

        (string, int) LuaGetUnitCastingInfo(WowLuaUnit target);

        int LuaGetUnspentTalentPoints();

        bool LuaIsInLfgGroup();

        void LuaLeaveBattleground();

        void LuaLootEveryThing();

        void LuaLootMoneyAndQuestItems();

        void LuaQueryQuestsCompleted();

        void LuaRepairAllItems();

        void LuaRepopMe();

        void LuaRetrieveCorpse();

        void LuaRollOnLoot(int rollId, WowRollType need);

        void LuaSelectGossipOption(int v);

        void LuaSelectQuestByNameOrGossipId(string name, int gossipId, bool v);

        void LuaSelectQuestLogEntry(int questLogId);

        void LuaSendChatMessage(string v);

        void LuaSetLfgRole(WowRole wowRole);

        void LuaSpellStopCasting();

        void LuaStartAutoAttack();

        void LuaUseContainerItem(int bagId, int bagSlot);

        void LuaUseInventoryItem(WowEquipmentSlot equipmentSlot);

        void LuaUseItemByName(string name);

        bool Setup();

        void Tick();

        void WowClearTarget();

        void WowClickOnTerrain(Vector3 position);

        void WowEnableClickToMove();

        bool WowExecuteLuaAndRead((string, string) p, out string result);

        void WowFacePosition(IntPtr playerBase, Vector3 playerPosition, Vector3 position);

        Dictionary<int, int> WowGetRunesReady();

        bool WowIsClickToMoveActive();

        bool WowIsInLineOfSight(Vector3 position1, Vector3 position2, float heightAdjust = 1.5f);

        bool WowIsRuneReady(int id);

        void WowObjectRightClick(IntPtr objectBase);

        void WowSetFacing(IntPtr playerBas, float angle);

        void WowSetRenderState(bool state);

        void WowStopClickToMove(IntPtr playerBase);

        void WowStopClickToMove();

        void WowTargetGuid(ulong guid);

        void WowUnitRightClick(IntPtr unitBase);
        void BotOverrideWorldLoadedCheck(bool enabled);
    }
}