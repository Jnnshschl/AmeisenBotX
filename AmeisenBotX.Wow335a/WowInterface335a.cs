using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Hook.Modules;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Events;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Events;
using AmeisenBotX.Wow335a.Hook;
using AmeisenBotX.Wow335a.Objects;
using AmeisenBotX.Wow335a.Offsets;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Wow335a
{
    /// <summary>
    /// WowInterface for the game version 3.3.5a 12340.
    /// </summary>
    public class WowInterface335a : IWowInterface
    {
        public WowInterface335a(IMemoryApi memoryApi)
        {
            OffsetList = new();
            HookModules = new();

            // lua variable names for the event hook
            string handlerName = BotUtils.FastRandomStringOnlyLetters();
            string tableName = BotUtils.FastRandomStringOnlyLetters();
            string eventHookOutput = BotUtils.FastRandomStringOnlyLetters();

            // name of the frame used to capture wows events
            string eventHookFrameName = BotUtils.FastRandomStringOnlyLetters();

            EventManager = new(LuaDoString, eventHookFrameName);

            // module to process wows events.
            HookModules.Add(new RunLuaHookModule
            (
                (x) => { if (memoryApi.ReadString(x, Encoding.UTF8, out string s) && !string.IsNullOrWhiteSpace(s)) { EventManager.OnEventPush(s); } },
                null,
                memoryApi,
                Offsets,
                $"{eventHookOutput}='['function {handlerName}(self,a,...)table.insert({tableName},{{time(),a,{{...}}}})end if {eventHookFrameName}==nil then {tableName}={{}}{eventHookFrameName}=CreateFrame(\"FRAME\"){eventHookFrameName}:SetScript(\"OnEvent\",{handlerName})else for b,c in pairs({tableName})do {eventHookOutput}={eventHookOutput}..'{{'for d,e in pairs(c)do if type(e)==\"table\"then {eventHookOutput}={eventHookOutput}..'\"args\": ['for f,g in pairs(e)do {eventHookOutput}={eventHookOutput}..'\"'..g..'\"'if f<=table.getn(e)then {eventHookOutput}={eventHookOutput}..','end end {eventHookOutput}={eventHookOutput}..']}}'if b<table.getn({tableName})then {eventHookOutput}={eventHookOutput}..','end else if type(e)==\"string\"then {eventHookOutput}={eventHookOutput}..'\"event\": \"'..e..'\",'else {eventHookOutput}={eventHookOutput}..'\"time\": \"'..e..'\",'end end end end end {eventHookOutput}={eventHookOutput}..']'{tableName}={{}}",
                eventHookOutput
            ));

            string staticPopupsVarName = BotUtils.FastRandomStringOnlyLetters();
            string oldPoupString = string.Empty;

            // module that monitors the STATIC_POPUP windows.
            HookModules.Add(new RunLuaHookModule
            (
                (x) =>
                {
                    if (memoryApi.ReadString(x, Encoding.UTF8, out string s)
                        && !string.IsNullOrWhiteSpace(s))
                    {
                        if (!oldPoupString.Equals(s, StringComparison.Ordinal))
                        {
                            OnStaticPopup?.Invoke(s);
                            oldPoupString = s;
                        }
                    }
                    else
                    {
                        oldPoupString = string.Empty;
                    }
                },
                null,
                memoryApi,
                Offsets,
                $"{staticPopupsVarName}=\"\"for b=1,STATICPOPUP_NUMDIALOGS do local c=_G[\"StaticPopup\"..b]if c:IsShown()then {staticPopupsVarName}={staticPopupsVarName}..b..\":\"..c.which..\"; \"end end",
                staticPopupsVarName
            ));

            string battlegroundStatusVarName = BotUtils.FastRandomStringOnlyLetters();
            string oldBattlegroundStatus = string.Empty;

            // module to monitor the battleground (and queue) status.
            HookModules.Add(new RunLuaHookModule
            (
                (x) =>
                {
                    if (memoryApi.ReadString(x, Encoding.UTF8, out string s)
                        && !string.IsNullOrWhiteSpace(s))
                    {
                        if (!oldBattlegroundStatus.Equals(s, StringComparison.Ordinal))
                        {
                            OnBattlegroundStatus?.Invoke(s);
                            oldBattlegroundStatus = s;
                        }
                    }
                    else
                    {
                        oldBattlegroundStatus = string.Empty;
                    }
                },
                null,
                memoryApi,
                Offsets,
                $"{battlegroundStatusVarName}=\"\"for b=1,MAX_BATTLEFIELD_QUEUES do local c,d,e,f,g,h=GetBattlefieldStatus(b)local i=GetBattlefieldTimeWaited(b)/1000;{battlegroundStatusVarName}={battlegroundStatusVarName}..b..\":\"..tostring(c or\"unknown\")..\":\"..tostring(d or\"unknown\")..\":\"..tostring(e or\"unknown\")..\":\"..tostring(f or\"unknown\")..\":\"..tostring(g or\"unknown\")..\":\"..tostring(h or\"unknown\")..\":\"..tostring(i or\"unknown\")..\";\"end",
                battlegroundStatusVarName
            ));

            // module to detect small obstacles that we can jump over
            HookModules.Add(new TracelineJumpHookModule
            (
                null,
                (x) =>
                {
                    if (Player != null)
                    {
                        Vector3 playerPosition = Player.Position;
                        playerPosition.Z += 1.3f;

                        Vector3 pos = BotUtils.MoveAhead(playerPosition, Player.Rotation, 0.25f);
                        memoryApi.Write(x.GetDataPointer(), (1.0f, playerPosition, pos));
                    }
                },
                memoryApi,
                Offsets
            ));

            ObjectManager = new(memoryApi, Offsets);

            Hook = new(memoryApi, Offsets, ObjectManager);
            Hook.OnGameInfoPush += ObjectManager.HookManagerOnGameInfoPush;
        }

        public event Action<string> OnBattlegroundStatus;

        public event Action<string> OnStaticPopup;

        public IEventManager Events => EventManager;

        public ulong HookCallCount => Hook.HookCallCount;

        public bool IsReady => Hook.IsWoWHooked;

        public IObjectProvider ObjectProvider => ObjectManager;

        public IOffsetList Offsets => OffsetList;

        public IWowPlayer Player => ObjectManager.Player;

        private SimpleEventManager EventManager { get; }

        private EndSceneHook Hook { get; }

        private List<IHookModule> HookModules { get; }

        private ObjectManager ObjectManager { get; }

        private OffsetList335a OffsetList { get; }

        public void Dispose()
        {
            Hook.Unhook();
        }

        public void LuaAbandonQuestsNotIn(IEnumerable<string> quests)
        {
            Hook.LuaAbandonQuestsNotIn(quests);
        }

        public void LuaAcceptBattlegroundInvite()
        {
            Hook.LuaAcceptBattlegroundInvite();
        }

        public void LuaAcceptPartyInvite()
        {
            Hook.LuaAcceptPartyInvite();
        }

        public void LuaAcceptQuest()
        {
            Hook.LuaAcceptQuest();
        }

        public void LuaAcceptQuests()
        {
            Hook.LuaAcceptQuests();
        }

        public void LuaAcceptResurrect()
        {
            Hook.LuaAcceptResurrect();
        }

        public void LuaAcceptSummon()
        {
            Hook.LuaAcceptSummon();
        }

        public bool LuaAutoLootEnabled()
        {
            return Hook.LuaAutoLootEnabled();
        }

        public void LuaCallCompanion(int index)
        {
            Hook.LuaCallCompanion(index);
        }

        public void LuaCastSpell(string spellName, bool castOnSelf = false)
        {
            Hook.LuaCastSpell(spellName, castOnSelf);
        }

        public void LuaCastSpellById(int spellId)
        {
            Hook.LuaCastSpellById(spellId);
        }

        public void LuaClickUiElement(string uiElement)
        {
            Hook.LuaClickUiElement(uiElement);
        }

        public void LuaCofirmLootRoll()
        {
            Hook.LuaCofirmLootRoll();
        }

        public void LuaCofirmReadyCheck(bool isReady)
        {
            Hook.LuaCofirmReadyCheck(isReady);
        }

        public void LuaCofirmStaticPopup()
        {
            Hook.LuaCofirmStaticPopup();
        }

        public void LuaCompleteQuest()
        {
            Hook.LuaCompleteQuest();
        }

        public void LuaDeleteInventoryItemByName(string name)
        {
            Hook.LuaDeleteInventoryItemByName(name);
        }

        public void LuaDismissCompanion()
        {
            Hook.LuaDismissCompanion();
        }

        public bool LuaDoString(string v)
        {
            return Hook.LuaDoString(v);
        }

        public void LuaEquipItem(string itemName, int slot = -1)
        {
            Hook.LuaEquipItem(itemName, slot);
        }

        public IEnumerable<int> LuaGetCompletedQuests()
        {
            return Hook.LuaGetCompletedQuests();
        }

        public string LuaGetEquipmentItems()
        {
            return Hook.LuaGetEquipmentItems();
        }

        public int LuaGetFreeBagSlotCount()
        {
            return Hook.LuaGetFreeBagSlotCount();
        }

        public string[] LuaGetGossipTypes()
        {
            return Hook.LuaGetGossipTypes();
        }

        public string LuaGetInventoryItems()
        {
            return Hook.LuaGetInventoryItems();
        }

        public string LuaGetItemJsonByNameOrLink(string itemLink)
        {
            return Hook.LuaGetItemJsonByNameOrLink(itemLink);
        }

        public string LuaGetLootRollItemLink(int rollId)
        {
            return Hook.LuaGetLootRollItemLink(rollId);
        }

        public string LuaGetMoney()
        {
            return Hook.LuaGetMoney();
        }

        public string LuaGetMounts()
        {
            return Hook.LuaGetMounts();
        }

        public bool LuaGetNumQuestLogChoices(out int numChoices)
        {
            return Hook.LuaGetNumQuestLogChoices(out numChoices);
        }

        public bool LuaGetQuestLogChoiceItemLink(int i, out string itemLink)
        {
            return Hook.LuaGetQuestLogChoiceItemLink(i, out itemLink);
        }

        public bool LuaGetQuestLogIdByTitle(string name, out int questLogId)
        {
            return Hook.LuaGetQuestLogIdByTitle(name, out questLogId);
        }

        public void LuaGetQuestReward(int i)
        {
            Hook.LuaGetQuestReward(i);
        }

        public Dictionary<string, (int, int)> LuaGetSkills()
        {
            return Hook.LuaGetSkills();
        }

        public double LuaGetSpellCooldown(string spellName)
        {
            return Hook.LuaGetSpellCooldown(spellName);
        }

        public string LuaGetSpellNameById(int spellId)
        {
            return Hook.LuaGetSpellNameById(spellId);
        }

        public string LuaGetSpells()
        {
            return Hook.LuaGetSpells();
        }

        public string LuaGetTalents()
        {
            return Hook.LuaGetTalents();
        }

        public (string, int) LuaGetUnitCastingInfo(WowLuaUnit target)
        {
            return Hook.LuaGetUnitCastingInfo(target.ToString());
        }

        public int LuaGetUnspentTalentPoints()
        {
            return Hook.LuaGetUnspentTalentPoints();
        }

        public bool LuaIsInLfgGroup()
        {
            return Hook.LuaIsInLfgGroup();
        }

        public void LuaLeaveBattleground()
        {
            Hook.LuaLeaveBattleground();
        }

        public void LuaLootEveryThing()
        {
            Hook.LuaLootEveryThing();
        }

        public void LuaLootMoneyAndQuestItems()
        {
            Hook.LuaLootMoneyAndQuestItems();
        }

        public void LuaQueryQuestsCompleted()
        {
            Hook.LuaQueryQuestsCompleted();
        }

        public void LuaRepairAllItems()
        {
            Hook.LuaRepairAllItems();
        }

        public void LuaRepopMe()
        {
            Hook.LuaRepopMe();
        }

        public void LuaRetrieveCorpse()
        {
            Hook.LuaRetrieveCorpse();
        }

        public void LuaRollOnLoot(int rollId, WowRollType rollType)
        {
            Hook.LuaRollOnLoot(rollId, (int)rollType);
        }

        public void LuaSelectGossipOption(int gossipId)
        {
            Hook.LuaSelectGossipOption(gossipId);
        }

        public void LuaSelectQuestByNameOrGossipId(string name, int gossipId, bool isAvailableQuest)
        {
            Hook.LuaSelectQuestByNameOrGossipId(name, gossipId, isAvailableQuest);
        }

        public void LuaSelectQuestLogEntry(int questLogId)
        {
            Hook.LuaSelectQuestLogEntry(questLogId);
        }

        public void LuaSendChatMessage(string msg)
        {
            Hook.LuaSendChatMessage(msg);
        }

        public void LuaSetLfgRole(WowRole wowRole)
        {
            Hook.LuaSetLfgRole((int)wowRole);
        }

        public void LuaSpellStopCasting()
        {
            Hook.LuaSpellStopCasting();
        }

        public void LuaStartAutoAttack()
        {
            Hook.LuaStartAutoAttack();
        }

        public bool LuaUiIsVisible(params string[] v)
        {
            return Hook.LuaUiIsVisible(v);
        }

        public void LuaUseContainerItem(int bagId, int bagSlot)
        {
            Hook.LuaUseContainerItem(bagId, bagSlot);
        }

        public void LuaUseInventoryItem(WowEquipmentSlot equipmentSlot)
        {
            Hook.LuaUseInventoryItem((int)equipmentSlot);
        }

        public void LuaUseItemByName(string name)
        {
            Hook.LuaUseItemByName(name);
        }

        public bool Setup()
        {
            return Hook.Hook(7, HookModules);
        }

        public void SetWorldLoadedCheck(bool enabled)
        {
            Hook.BotOverrideWorldLoadedCheck(enabled);
        }

        public void Tick()
        {
            if (ObjectManager.RefreshIsWorldLoaded())
            {
                ObjectManager.UpdateWowObjects();
            }
        }

        public void WowClearTarget()
        {
            Hook.WowClearTarget();
        }

        public void WowClickOnTerrain(Vector3 position)
        {
            Hook.WowClickOnTerrain(position);
        }

        public void WowEnableClickToMove()
        {
            Hook.WowEnableClickToMove();
        }

        public bool WowExecuteLuaAndRead((string, string) p, out string result)
        {
            return Hook.WowExecuteLuaAndRead(p, out result);
        }

        public void WowFacePosition(IntPtr playerBase, Vector3 playerPosition, Vector3 position)
        {
            Hook.WowFacePosition(playerBase, playerPosition, position);
        }

        public WowUnitReaction WowGetReaction(IntPtr a, IntPtr b)
        {
            return (WowUnitReaction)Hook.WowGetUnitReaction(a, b);
        }

        public Dictionary<int, int> WowGetRunesReady()
        {
            return Hook.WowGetRunesReady();
        }

        public bool WowIsClickToMoveActive()
        {
            return Hook.WowIsClickToMoveActive();
        }

        public bool WowIsInLineOfSight(Vector3 position1, Vector3 position2, float heightAdjust = 1.5F)
        {
            return Hook.WowIsInLineOfSight(position1, position2, heightAdjust);
        }

        public bool WowIsRuneReady(int id)
        {
            return Hook.WowIsRuneReady(id);
        }

        public void WowObjectRightClick(IntPtr objectBase)
        {
            Hook.WowObjectRightClick(objectBase);
        }

        public void WowSetFacing(IntPtr playerBase, float angle)
        {
            Hook.WowSetFacing(playerBase, angle);
        }

        public void WowSetRenderState(bool state)
        {
            Hook.WowSetRenderState(state);
        }

        public void WowStopClickToMove(IntPtr playerBase)
        {
            Hook.WowStopClickToMove(playerBase);
        }

        public void WowStopClickToMove()
        {
            Hook.WowStopClickToMove(Player.BaseAddress);
        }

        public void WowTargetGuid(ulong guid)
        {
            Hook.WowTargetGuid(guid);
        }

        public void WowUnitRightClick(IntPtr unitBase)
        {
            Hook.WowUnitRightClick(unitBase);
        }
    }
}