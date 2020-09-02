using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Structs;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AmeisenBotX.Core.Hook
{
    public interface IHookManager
    {
        ulong HookCallCount { get; }

        bool IsWoWHooked { get; }

        void AcceptBattlegroundInvite();

        void AcceptPartyInvite();

        void AcceptQuest(int gossipId);

        void AcceptResurrect();

        void AcceptSummon();

        void AutoAcceptQuests();

        void CancelSummon();

        int CastAndGetSpellCooldown(string spellName, bool castOnSelf = false);

        void CastSpell(string name, bool castOnSelf = false);

        void CastSpellById(int spellId);

        void ClearTarget();

        void ClickOnTerrain(Vector3 position);

        void ClickToMove(WowPlayer player, Vector3 position);

        void ClickUiElement(string elementName);

        void CofirmBop();

        void CofirmLootRoll();

        void CofirmReadyCheck(bool isReady);

        void CompleteQuestAndGetReward(int questlogId, int rewardId, int gossipId);

        void DeclinePartyInvite();

        void DeclineResurrect();

        void Dismount();

        void DisposeHook();

        void EnableClickToMove();

        bool ExecuteLuaAndRead((string, string) commandVariableTuple, out string result);

        bool ExecuteLuaAndRead(string command, string variable, out string result);

        void FacePosition(WowPlayer player, Vector3 positionToFace);

        List<int> GetCompletedQuests();

        string GetEquipmentItems();

        int GetFreeBagSlotCount();

        int GetGossipOptionCount();

        string GetInventoryItems();

        string GetItemBySlot(int itemslot);

        string GetItemJsonByNameOrLink(string itemName);

        string GetItemStats(string itemLink);

        bool GetLocalizedText(string variable, out string result);

        string GetLootRollItemLink(int rollId);

        string GetMoney();

        string GetMounts();

        void GetQuestReward(int id);

        Dictionary<RuneType, int> GetRunesReady();

        Dictionary<string, (int, int)> GetSkills();

        /// <summary>
        /// Get the spells cooldown left in milliseconds
        /// </summary>
        /// <param name="spellName"></param>
        /// <returns>ms cooldown left</returns>
        int GetSpellCooldown(string spellName);

        string GetSpellNameById(int spellId);

        string GetSpells();

        string GetTalents();

        public void GetUnitAuras(IntPtr baseAddress, ref WowAura[] auraTable, out int auraCount);

        (string, int) GetUnitCastingInfo(WowLuaUnit luaunit);

        WowUnitReaction GetUnitReaction(WowUnit wowUnitA, WowUnit wowUnitB);

        int GetUnspentTalentPoints();

        bool HasUnitStealableBuffs(WowLuaUnit luaUnit);

        bool IsBgInviteReady();

        bool IsClickToMoveActive();

        bool IsGhost(WowLuaUnit luaUnit);

        bool IsInLfgGroup();

        bool IsInLineOfSight(Vector3 start, Vector3 end, float heightAdjust = 1.5f);

        bool IsOutdoors();

        bool IsRuneReady(int runeId);

        bool IsSpellKnown(int spellId, bool isPetSpell = false);

        void KickNpcsOutOfVehicle();

        void LearnAllAvaiableSpells();

        void LeaveBattleground();

        void LootEveryThing();

        void LootOnlyMoneyAndQuestItems();

        bool LuaDoString(string command);

        void Mount(int index);

        void OverrideWorldLoadedCheck(bool status);

        void QueryQuestsCompleted();

        void QueueBattlegroundByName(string bgName);

        void ReleaseSpirit();

        void RepairAllItems();

        void ReplaceItem(IWowItem currentItem, IWowItem newItem);

        void RetrieveCorpse();

        void RollOnItem(int rollId, RollType rollType);

        void SelectLfgRole(CombatClassRole combatClassRole);

        void SellAllItems();

        void SellItemsByName(string itemName);

        void SellItemsByQuality(ItemQuality itemQuality);

        void SendChatMessage(string message);

        void SendItemMailToCharacter(string itemName, string receiver);

        void SetFacing(WowUnit unit, float angle);

        void SetRenderState(bool renderingEnabled);

        bool SetupEndsceneHook();

        void StartAutoAttack();

        void StopCasting();

        void StopClickToMoveIfActive();

        void TargetGuid(ulong guid);

        void TargetLuaUnit(WowLuaUnit unit);

        byte TraceLine(Vector3 start, Vector3 end, out Vector3 result, uint flags = 0x120171);

        void UnitOnRightClick(WowUnit wowUnit);

        void UnitSelectGossipOption(int gossipId);

        void UseInventoryItem(EquipmentSlot equipmentSlot);

        void UseItemByBagAndSlot(int bagId, int bagSlot);

        void UseItemByName(string itemName);

        void WowObjectOnRightClick(WowObject gObject);
    }
}