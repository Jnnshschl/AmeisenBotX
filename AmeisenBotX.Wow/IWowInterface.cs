using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Cache;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow
{
    /// <summary>
    /// Interface to the wow game. All functions that interact with the game should be reachable via this interface.
    /// </summary>
    public interface IWowInterface
    {
        /// <summary>
        /// Gets fired when the battleground state changes.
        /// </summary>
        event Action<string> OnBattlegroundStatus;

        /// <summary>
        /// Gets fired when new events are read from wow.
        /// Format: JSON array of wow events.
        /// </summary>
        event Action<string> OnEventPush;

        /// <summary>
        /// Gets fired when a new static popup appears ingame.
        /// Format: {ID}:{POPUPTYPE};... => 1:DELETE_ITEM;2:SAMPLE_POPUP;...
        /// </summary>
        event Action<string> OnStaticPopup;

        /// <summary>
        /// Used for the HookCall display in the bots main window.
        /// Use this to display cost intensive calls to the user.
        /// The name Hookcall originates from EndScene hook calls.
        /// </summary>
        ulong HookCallCount { get; }

        /// <summary>
        /// Get the status of the wow interface, true if its useable, false if not.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Shortcut to get the last targets guid.
        /// </summary>
        public ulong LastTargetGuid => ObjectProvider.LastTarget != null ? ObjectProvider.LastTarget.Guid : 0ul;

        /// <summary>
        /// Use this to interact with wow's objects, units, players and more.
        /// </summary>
        IObjectProvider ObjectProvider { get; }

        /// <summary>
        /// Shortcut to all wow objects.
        /// </summary>
        IEnumerable<WowObject> Objects => ObjectProvider.WowObjects;

        /// <summary>
        /// Shortcut to get the current partyleaders guid.
        /// </summary>
        ulong PartyleaderGuid => ObjectProvider.Partyleader != null ? ObjectProvider.Partyleader.Guid : 0ul;

        /// <summary>
        /// Shortcut to get the current pets guid.
        /// </summary>
        public ulong PetGuid => ObjectProvider.Pet != null ? ObjectProvider.Pet.Guid : 0ul;

        /// <summary>
        /// Shortcut to get the current players guid.
        /// </summary>
        public ulong PlayerGuid => ObjectProvider.Player != null ? ObjectProvider.Player.Guid : 0ul;

        /// <summary>
        /// Shortcut to get the current targets guid.
        /// </summary>
        public ulong TargetGuid => ObjectProvider.Target != null ? ObjectProvider.Target.Guid : 0ul;

        /// <summary>
        /// Dispose the wow interface making it realese and unhook all resources.
        /// </summary>
        void Dispose();

        void LuaAbandonQuestsNotIn(IEnumerable<string> enumerable);

        void LuaAcceptBattlegroundInvite();

        void LuaAcceptPartyInvite();

        void LuaAcceptQuest();

        void LuaAcceptQuests();

        void LuaAcceptResurrect();

        void LuaAcceptSummon();

        bool LuaAutoLootEnabled();

        void LuaCallCompanion(int index);

        void LuaCastSpell(string spellName, bool castOnSelf = false);

        void LuaCastSpellById(int spellId);

        void LuaClickUiElement(string elementName);

        void LuaCofirmLootRoll();

        void LuaCofirmReadyCheck(bool isReady);

        void LuaCofirmStaticPopup();

        void LuaCompleteQuest();

        void LuaDeleteInventoryItemByName(string name);

        void LuaDismissCompanion();

        bool LuaDoString(string lua);

        void LuaEquipItem(string itemName, int slot = -1);

        IEnumerable<int> LuaGetCompletedQuests();

        string LuaGetEquipmentItems();

        int LuaGetFreeBagSlotCount();

        string[] LuaGetGossipTypes();

        string LuaGetInventoryItems();

        string LuaGetItemJsonByNameOrLink(string itemLink);

        string LuaGetLootRollItemLink(int rollId);

        string LuaGetMoney();

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

        void LuaSelectGossipOption(int i);

        void LuaSelectQuestByNameOrGossipId(string name, int gossipId, bool isAvailable);

        void LuaSelectQuestLogEntry(int questLogId);

        void LuaSendChatMessage(string msg);

        void LuaSetLfgRole(WowRole wowRole);

        void LuaSpellStopCasting();

        void LuaStartAutoAttack();

        bool LuaUiIsVisible(params string[] elementName);

        void LuaUseContainerItem(int bagId, int bagSlot);

        void LuaUseInventoryItem(WowEquipmentSlot equipmentSlot);

        void LuaUseItemByName(string name);

        /// <summary>
        /// Init the wow interface.
        /// </summary>
        /// <returns>True if everything went well, false if not</returns>
        bool Setup();

        /// <summary>
        /// Use this to diable the is world loaded check that is used to
        /// prevent the execution of assembly code during loading screens.
        /// Used to disable the check in the login process as the world
        /// is not loaded in the main menu.
        /// </summary>
        /// <param name="enabled">Status of the check (true = on | false = off)</param>
        void SetWorldLoadedCheck(bool enabled);

        /// <summary>
        /// Poll this on a regular basis to keep the stuff up to date.
        /// Updates objects, gameinfo and more.
        /// </summary>
        void Tick();

        void WowClearTarget();

        void WowClickOnTerrain(Vector3 position);

        void WowEnableClickToMove();

        bool WowExecuteLuaAndRead((string, string) commandVariableCombo, out string result);

        void WowFacePosition(IntPtr playerBase, Vector3 playerPosition, Vector3 position);

        WowUnitReaction WowGetReaction(IntPtr a, IntPtr b);

        Dictionary<int, int> WowGetRunesReady();

        bool WowIsClickToMoveActive();

        bool WowIsInLineOfSight(Vector3 a, Vector3 b, float heightAdjust = 1.5f);

        bool WowIsRuneReady(int id);

        void WowObjectRightClick(IntPtr objectBase);

        void WowSetFacing(IntPtr playerBas, float angle);

        void WowSetRenderState(bool state);

        void WowStopClickToMove(IntPtr playerBase);

        void WowStopClickToMove();

        void WowTargetGuid(ulong guid);

        void WowUnitRightClick(IntPtr unitBase);
    }
}