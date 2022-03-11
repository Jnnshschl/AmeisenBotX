using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Events;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Constants;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Offsets;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow
{
    /// <summary>
    /// Interface to the wow game. All functions that interact with the game should be reachable via
    /// this interface.
    /// </summary>
    public interface IWowInterface
    {
        /// <summary>
        /// Gets fired when the battleground state changes.
        /// </summary>
        event Action<string> OnBattlegroundStatus;

        /// <summary>
        /// Gets fired when a new static popup appears ingame.
        /// Format: {ID}:{POPUPTYPE};... =&gt; 1:DELETE_ITEM;2:SAMPLE_POPUP;...
        /// </summary>
        event Action<string> OnStaticPopup;

        /// <summary>
        /// Use this to interact with the wow event system.
        /// </summary>
        IEventManager Events { get; }

        /// <summary>
        /// Used for the HookCall display in the bots main window. Use this to display cost
        /// intensive calls to the user. The name Hookcall originates from EndScene hook calls.
        /// </summary>
        int HookCallCount { get; }

        /// <summary>
        /// Get the status of the wow interface, true if its useable, false if not.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Shortcut to get the last targets guid.
        /// </summary>
        public ulong LastTargetGuid => ObjectProvider.LastTarget != null ? ObjectProvider.LastTarget.Guid : 0ul;

        /// <summary>
        /// Use this to interact with wowobjects, units, players and more.
        /// </summary>
        IObjectProvider ObjectProvider { get; }

        /// <summary>
        /// Shortcut to all wow objects.
        /// </summary>
        IEnumerable<IWowObject> Objects => ObjectProvider.WowObjects;

        /// <summary>
        /// Currently used offset list.
        /// </summary>
        IOffsetList Offsets { get; }

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
        /// Get the current version of wow.
        /// </summary>
        WowVersion WowVersion { get; }

        void AbandonQuestsNotIn(IEnumerable<string> enumerable);

        void AcceptBattlegroundInvite();

        void AcceptPartyInvite();

        void AcceptQuest();

        void AcceptQuests();

        void AcceptResurrect();

        void AcceptSummon();

        void CallCompanion(int index, string type);

        /// <summary>
        /// Cast a spell using the lua CastSpell function.
        /// </summary>
        /// <param name="spellName">Name of the spell to cast</param>
        /// <param name="castOnSelf">True if we should cast it on our own character</param>
        void CastSpell(string spellName, bool castOnSelf = false);

        void CastSpellById(int spellId);

        void ChangeTarget(ulong guid);

        void ClearTarget();

        void ClickOnTerrain(Vector3 position);

        void ClickOnTrainButton();

        void ClickToMove(Vector3 pos, ulong guid, WowClickToMoveType clickToMoveType = WowClickToMoveType.Move, float turnSpeed = 20.9f, float distance = WowClickToMoveDistance.Move);

        /// <summary>
        /// Performs a click on the given ui element.
        /// </summary>
        /// <param name="elementName">UI element name, find it using ingame command "/fstack"</param>
        void ClickUiElement(string elementName);

        void CofirmLootRoll();

        void CofirmReadyCheck(bool isReady);

        void CofirmStaticPopup();

        void CompleteQuest();

        void DeleteItemByName(string name);

        void DismissCompanion(string type);

        /// <summary>
        /// Dispose the wow interface making it realese and unhook all resources.
        /// </summary>
        void Dispose();

        void EquipItem(string itemName, int slot = -1);

        bool ExecuteLuaAndRead((string, string) commandVariableCombo, out string result);

        void FacePosition(IntPtr playerBase, Vector3 playerPosition, Vector3 position, bool smooth = false);

        IEnumerable<int> GetCompletedQuests();

        string GetEquipmentItems();

        /// <summary>
        /// Returns the number of unused bag slots.
        /// </summary>
        /// <returns>Free bag slot count</returns>
        int GetFreeBagSlotCount();

        string[] GetGossipTypes();

        string GetInventoryItems();

        string GetItemByNameOrLink(string itemLink);

        string GetLootRollItemLink(int rollId);

        /// <summary>
        /// Returns the current money in copper.
        /// </summary>
        /// <returns>Money in copper</returns>
        int GetMoney();

        IEnumerable<WowMount> GetMounts();

        bool GetNumQuestLogChoices(out int numChoices);

        bool GetQuestLogChoiceItemLink(int i, out string itemLink);

        bool GetQuestLogIdByTitle(string name, out int questLogId);

        WowUnitReaction GetReaction(IntPtr a, IntPtr b);

        Dictionary<int, int> GetRunesReady();

        Dictionary<string, (int, int)> GetSkills();

        int GetSpellCooldown(string spellName);

        string GetSpellNameById(int spellId);

        string GetSpells();

        string GetTalents();

        (string, int) GetUnitCastingInfo(WowLuaUnit target);

        int GetUnspentTalentPoints();

        void InteractWithObject(IWowObject obj);

        void InteractWithUnit(IWowUnit unit);

        /// <summary>
        /// Gets the state of autoloot.
        /// </summary>
        /// <returns>True if it is enabled, false if not</returns>
        bool IsAutoLootEnabled();

        bool IsClickToMoveActive();

        bool IsInLfgGroup();

        bool IsInLineOfSight(Vector3 a, Vector3 b, float heightAdjust = 1.5f);

        bool IsRuneReady(int id);

        void LeaveBattleground();

        void LootEverything();

        void LootMoneyAndQuestItems();

        /// <summary>
        /// Run lua code in wow using the LuaDosString() function
        /// </summary>
        /// <param name="lua">Code to run</param>
        /// <returns>Whether the code was executed or not</returns>
        bool LuaDoString(string lua);

        void LuaQueueBattlegroundByName(string bgName);

        void QueryQuestsCompleted();

        void RepairAllItems();

        void RepopMe();

        void RetrieveCorpse();

        void RollOnLoot(int rollId, WowRollType need);

        void SelectGossipActiveQuest(int gossipId);

        void SelectGossipAvailableQuest(int gossipId);

        void SelectGossipOption(int i);

        void SelectGossipOptionSimple(int i);

        void SelectQuestByNameOrGossipId(string name, int gossipId, bool isAvailable);

        void SelectQuestLogEntry(int questLogId);

        void SelectQuestReward(int i);

        void SendChatMessage(string msg);

        void SetFacing(IntPtr playerBase, float angle, bool smooth = false);

        void SetLfgRole(WowRole wowRole);

        void SetRenderState(bool state);

        /// <summary>
        /// Init the wow interface.
        /// </summary>
        /// <returns>True if everything went well, false if not</returns>
        bool Setup();

        /// <summary>
        /// Use this to diable the is world loaded check that is used to prevent the execution of
        /// assembly code during loading screens. Used to disable the check in the login process as
        /// the world is not loaded in the main menu.
        /// </summary>
        /// <param name="enabled">Status of the check (true = on | false = off)</param>
        void SetWorldLoadedCheck(bool enabled);

        void StartAutoAttack();

        void StopCasting();

        void StopClickToMove();

        /// <summary>
        /// Poll this on a regular basis to keep the stuff up to date. Updates objects, gameinfo and more.
        /// </summary>
        void Tick();

        bool UiIsVisible(params string[] elementNames);

        void UseContainerItem(int bagId, int bagSlot);

        void UseInventoryItem(WowEquipmentSlot equipmentSlot);

        void UseItemByName(string name);
    }
}