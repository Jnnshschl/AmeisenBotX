using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Hook.Modules;
using AmeisenBotX.Core.Hook.Structs;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Hook
{
    /// <summary>
    /// This is the main interactionpoint with the wow game it
    /// handles function calling, execution of asm code and
    /// hooking the EndScene.
    /// 
    /// Functions starting with Lua are mostly shortcut calls
    /// of LuaDostring(). Many of them reflect wows lua API.
    /// 
    /// Functions starting with Wow are native functions of
    /// wows engine, for example TraceLine().
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

        /// <summary>
        /// Use this function to disable the is world loaded 
        /// safety check. Useful when you want to execute
        /// asm in the main menu or character selection.
        /// </summary>
        /// <param name="status">True will disable the check, false will enable it</param>
        void BotOverrideWorldLoadedCheck(bool status);

        /// <summary>
        /// Hook wows EndScene function to execute asm on its
        /// main thread. This is needed for the majority of
        /// functions inside this class. Make sure to call
        /// it only once.
        /// </summary>
        /// <param name="hookSize">How many byte the hook should overwrite, needs to be aleast 5 (JMP instruction + Address)</param>
        /// <param name="hookModules">List of HookModules to load</param>
        /// <returns>True when the hook was successful, false if not</returns>
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
        /// Get the spells cooldown left in milliseconds.
        /// </summary>
        /// <param name="spellName">Name of the spell you want to check</param>
        /// <returns>Ms cooldown left</returns>
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

        /// <summary>
        /// Use this function to dispose the hook.
        /// </summary>
        void Unhook();

        /// <summary>
        /// Clears the current target by calling SetTarget(0);
        /// </summary>
        void WowClearTarget();

        /// <summary>
        /// Use this function to place AEO spells on the ground.
        /// </summary>
        /// <param name="position"></param>
        void WowClickOnTerrain(Vector3 position);

        /// <summary>
        /// Call the click to move function for a player, only call this with the local player.
        /// </summary>
        /// <param name="player">Local player</param>
        /// <param name="position">Position to move</param>
        void WowClickToMove(WowPlayer player, Vector3 position);

        /// <summary>
        /// Enable the CTM function of wow.
        /// </summary>
        void WowEnableClickToMove();

        /// <summary>
        /// This function executes the given command and reads the 
        /// returned value from the supplied variable
        /// </summary>
        /// <param name="commandVariableTuple">Tuple of command and variable</param>
        /// <param name="result">Return value</param>
        /// <returns>True when the command execution was successful, false if not</returns>
        bool WowExecuteLuaAndRead((string, string) commandVariableTuple, out string result);

        /// <summary>
        /// This function executes the given command and reads the 
        /// returned value from the supplied variable
        /// </summary>
        /// <param name="command">Lua command to execute</param
        /// <param name="variable">Variable to read the return value from</param>
        /// <param name="result">Return value</param>
        /// <returns>True when the command execution was successful, false if not</returns>
        bool WowExecuteLuaAndRead(string command, string variable, out string result);

        /// <summary>
        /// Face a specific position, only call this with the local player.
        /// </summary>
        /// <param name="player">Local Player</param>
        /// <param name="positionToFace">Position to face</param>
        void WowFacePosition(WowPlayer player, Vector3 positionToFace);

        /// <summary>
        /// Read a LUA variable from wow.
        /// </summary>
        /// <param name="variable">Variable to read</param>
        /// <param name="result">Variable value</param>
        /// <returns>True if the variable reading was successful, false if not</returns>
        bool WowGetLocalizedText(string variable, out string result);

        /// <summary>
        /// Returns a dict containing all rune types and how many of them are ready.
        /// </summary>
        /// <returns></returns>
        Dictionary<WowRuneType, int> WowGetRunesReady();

        /// <summary>
        /// Read the aura table of a unit.
        /// </summary>
        /// <param name="unit">Unit to read the aura table from</param>
        /// <param name="auraCount">Size of the aura table</param>
        /// <returns>The aura table as a list</returns>
        IEnumerable<WowAura> WowGetUnitAuras(WowUnit unit, out int auraCount);

        /// <summary>
        /// Get the reaction of a unit to another.
        /// </summary>
        /// <param name="wowUnitA">Unit a</param>
        /// <param name="wowUnitB">Unit b</param>
        /// <returns>The reaction of unit a to unit b</returns>
        WowUnitReaction WowGetReaction(WowUnit wowUnitA, WowUnit wowUnitB);

        /// <summary>
        /// Checks the CTM status.
        /// </summary>
        /// <returns>Wether it is active or not</returns>
        bool WowIsClickToMoveActive();

        /// <summary>
        /// Check whether the start position can see the end position.
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">Target position</param>
        /// <param name="heightAdjust">1.5f is default for units</param>
        /// <returns>True is it is in LOS, false is not</returns>
        bool WowIsInLineOfSight(Vector3 start, Vector3 end, float heightAdjust = 1.5f);

        /// <summary>
        /// Check whether a specific rune is ready.
        /// </summary>
        /// <param name="runeId">Ranges from 0 to 5</param>
        /// <returns>Wether the rune is ready or not</returns>
        bool WowIsRuneReady(int runeId);

        /// <summary>
        /// Perform a right click on a WoWObject. Only us this for Gameobjects
        /// </summary>
        /// <param name="gObject"></param>
        void WowObjectRightClick(WowObject gObject);

        /// <summary>
        /// Rotate localplayer to a given angle, only call this with the local player.
        /// </summary>
        /// <param name="unit">Local player</param>
        /// <param name="angle">0 to (2 * PI)</param>
        void WowSetFacing(WowUnit unit, float angle);

        /// <summary>
        /// Disable/Enable the rendering of wows engine, may cause graphics.
        /// </summary>
        /// <param name="renderingEnabled">Wether wow should render stuff or not</param>
        void WowSetRenderState(bool renderingEnabled);

        /// <summary>
        /// Stops the current click to move execution. Makes the player stop.
        /// </summary>
        void WowStopClickToMove();

        /// <summary>
        /// Target a specific unit by its guid.
        /// </summary>
        /// <param name="guid">Unit guid</param>
        void WowTargetGuid(ulong guid);

        /// <summary>
        /// Trace a line in the wow 3d space and see whether it collides with an object or not.
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="result">Hit position</param>
        /// <param name="flags">Collision flags</param>
        /// <returns>Trace line result</returns>
        byte WowTraceLine(Vector3 start, Vector3 end, out Vector3 result, uint flags = 0x120171);

        /// <summary>
        /// Right click a Unit, only call this on Units and Players, not objects.
        /// </summary>
        /// <param name="wowUnit">Unit to perform rightclick on</param>
        void WowUnitRightClick(WowUnit wowUnit);
    }
}