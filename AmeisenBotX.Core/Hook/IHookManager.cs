using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Hook
{
    public interface IHookManager
    {
        ulong CallCount { get; }

        public bool IsWoWHooked { get; }

        void AcceptBattlegroundInvite();

        void AcceptPartyInvite();

        void AcceptQuest(int gossipId);

        void AcceptResurrect();

        void AcceptSummon();

        void CastSpell(string name, bool castOnSelf = false);

        void CastSpellById(int spellId);

        void ClearTarget();

        void ClickOnTerrain(Vector3 position);

        void ClickToMove(WowPlayer player, Vector3 position);

        void ClickUiElement(string elementName);

        void CofirmBop();

        void CofirmReadyCheck(bool isReady);

        void CompleteQuestAndGetReward(int questlogId, int rewardId);

        void DisposeHook();

        void EnableClickToMove();

        string ExecuteLuaAndRead(string command, string variable);

        void FacePosition(WowPlayer player, Vector3 positionToFace);

        List<string> GetAuras(WowLuaUnit luaunit);

        List<string> GetBuffs(WowLuaUnit luaunit);

        List<int> GetCompletedQuests();

        List<string> GetDebuffs(WowLuaUnit luaunit);

        string GetEquipmentItems();

        int GetFreeBagSlotCount();

        string GetInventoryItems();

        string GetItemBySlot(int itemslot);

        string GetItemJsonByNameOrLink(string itemName);

        string GetItemStats(string itemLink);

        string GetLocalizedText(string variable);

        string GetLootRollItemLink(int rollId);

        string GetMoney();

        Dictionary<RuneType, int> GetRunesReady();

        List<string> GetSkills();

        double GetSpellCooldown(string spellName);

        string GetSpells();

        string GetTalents();

        List<WowAura> GetUnitAuras(WowUnit wowUnit);

        (string, int) GetUnitCastingInfo(WowLuaUnit luaunit);

        WowUnitReaction GetUnitReaction(WowUnit wowUnitA, WowUnit wowUnitB);

        int GetUnspentTalentPoints();

        bool HasUnitStealableBuffs(WowLuaUnit luaUnit);

        bool IsBgInviteReady();

        bool IsClickToMoveActive(WowPlayer player);

        bool IsGhost(WowLuaUnit luaUnit);

        bool IsInLfgGroup();

        bool IsInLineOfSight(Vector3 start, Vector3 end, float heightAdjust = 1.5f);

        bool IsRuneReady(int runeId);

        bool IsSpellKnown(int spellId, bool isPetSpell = false);

        void KickNpcsOutOfMammoth();

        void LearnAllAvaiableSpells();

        void LeaveBattleground();

        void LootEveryThing();

        void LuaDoString(string command);

        void OverrideWorldCheckOff();

        void OverrideWorldCheckOn();

        void QueryQuestsCompleted();

        void QueueBattlegroundByName(string bgName);

        void ReleaseSpirit();

        void RepairAllItems();

        void ReplaceItem(IWowItem currentItem, IWowItem newItem);

        void RetrieveCorpse();

        void RollOnItem(int rollId, RollType rollType);

        void SelectLfgRole(CombatClassRole combatClassRole);

        void SellAllGrayItems();

        void SellAllItems();

        void SellItemsByName(string itemName);

        void SendChatMessage(string message);

        void SendItemMailToCharacter(string itemName, string receiver);

        void SendMovementPacket(WowUnit unit, int opcode);

        void SetFacing(WowUnit unit, float angle);

        void SetMaxFps(byte maxFps);

        void SetRenderState(bool renderingEnabled);

        bool SetupEndsceneHook();

        void StartAutoAttack(WowUnit wowUnit);

        void StopClickToMoveIfActive();

        void TargetGuid(ulong guid);

        void TargetLuaUnit(WowLuaUnit unit);

        byte TraceLine(Vector3 start, Vector3 end, out Vector3 result, uint flags = 0x120171);

        void UnitOnRightClick(WowUnit wowUnit);

        void UseInventoryItem(EquipmentSlot equipmentSlot);

        void UseItemByBagAndSlot(int bagId, int bagSlot);

        void UseItemByName(string itemName);

        void WowObjectOnRightClick(WowObject gObject);
    }
}