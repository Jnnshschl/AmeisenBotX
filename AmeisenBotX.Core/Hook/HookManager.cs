using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Data.Objects.Structs;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Pathfinding.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AmeisenBotX.Core.Hook
{
    public class HookManager : IHookManager
    {
        private const int ENDSCENE_HOOK_OFFSET = 0x2;

        public HookManager(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
        }

        public IntPtr CodecaveForCheck { get; private set; }

        public IntPtr CodecaveForExecution { get; private set; }

        public IntPtr CodeToExecuteAddress { get; private set; }

        public IntPtr EndsceneAddress { get; private set; }

        public IntPtr EndsceneReturnAddress { get; private set; }

        public bool IsInjectionUsed { get; private set; }

        public bool IsWoWHooked
        {
            get
            {
                if (WowInterface.XMemory.ReadByte(EndsceneAddress, out byte c))
                {
                    return c == 0xE9;
                }
                else
                {
                    return false;
                }
            }
        }

        public byte[] OriginalEndsceneBytes { get; private set; }

        public IntPtr ReturnValueAddress { get; private set; }

        private WowInterface WowInterface { get; }

        public void AcceptBattlegroundInvite()
            => SendChatMessage("/click StaticPopup1Button1");

        public void AcceptPartyInvite()
            => LuaDoString("AcceptGroup();");

        public void AcceptResurrect()
            => LuaDoString("AcceptResurrect();");

        public void AcceptSummon()
            => LuaDoString("ConfirmSummon();");

        public void CastSpell(string name, bool castOnSelf = false)
        {
            AmeisenLogger.Instance.Log("HookManager", $"Casting spell with name: {name}", LogLevel.Verbose);

            if (castOnSelf)
            {
                LuaDoString($"CastSpellByName(\"{name}\", true);");
            }
            else
            {
                LuaDoString($"CastSpellByName(\"{name}\");");
            }
        }

        public void CastSpellById(int spellId)
        {
            AmeisenLogger.Instance.Log("HookManager", $"Casting spell with id: {spellId}", LogLevel.Verbose);

            if (spellId > 0)
            {
                string[] asm = new string[]
                {
                    "PUSH 0",
                    "PUSH 0",
                    "PUSH 0",
                    $"PUSH {spellId}",
                    $"CALL {WowInterface.OffsetList.FunctionCastSpellById.ToInt32()}",
                    "ADD ESP, 0x10",
                };

                InjectAndExecute(asm, false);
            }
        }

        public void ClearTarget()
            => SendChatMessage("/cleartarget");

        public void ClearTargetIfDead()
            => SendChatMessage("/cleartarget [dead]");

        public void ClickOnTerrain(Vector3 position)
        {
            if (WowInterface.XMemory.AllocateMemory(20, out IntPtr codeCaveVector3))
            {
                WowInterface.XMemory.Write<ulong>(codeCaveVector3, 0);
                WowInterface.XMemory.Write(IntPtr.Add(codeCaveVector3, 0x8), position);

                string[] asm = new string[]
                {
                    $"PUSH {codeCaveVector3.ToInt32()}",
                    $"CALL {WowInterface.OffsetList.FunctionHandleTerrainClick}",
                    "ADD ESP, 0x4",
                    "RETN",
                };

                InjectAndExecute(asm, false);
                WowInterface.XMemory.FreeMemory(codeCaveVector3);
            }
        }

        public void ClickToMove(WowPlayer player, Vector3 position)
        {
            if (WowInterface.XMemory.AllocateMemory(12, out IntPtr codeCaveVector3))
            {
                WowInterface.XMemory.Write(codeCaveVector3, position);

                CallObjectFunction(player.BaseAddress, WowInterface.OffsetList.FunctionPlayerClickToMove, new List<object>() { codeCaveVector3.ToInt32() });
                WowInterface.XMemory.FreeMemory(codeCaveVector3);
            }
        }

        public void CofirmBop()
        {
            LuaDoString("ConfirmBindOnUse();");
            SendChatMessage("/click StaticPopup1Button1");
        }

        public void CofirmReadyCheck(bool isReady)
            => LuaDoString($"ConfirmReadyCheck({isReady});");

        public void DisposeHook()
        {
            if (IsWoWHooked)
            {
                AmeisenLogger.Instance.Log("HookManager", "Disposing EnsceneHook...", LogLevel.Verbose);
                WowInterface.XMemory.WriteBytes(EndsceneAddress, OriginalEndsceneBytes);

                if (CodecaveForCheck != null)
                {
                    WowInterface.XMemory.FreeMemory(CodecaveForCheck);
                }

                if (CodecaveForExecution != null)
                {
                    WowInterface.XMemory.FreeMemory(CodecaveForExecution);
                }

                if (CodeToExecuteAddress != null)
                {
                    WowInterface.XMemory.FreeMemory(CodeToExecuteAddress);
                }

                if (ReturnValueAddress != null)
                {
                    WowInterface.XMemory.FreeMemory(ReturnValueAddress);
                }
            }
        }

        public void EnableClickToMove()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.ClickToMovePointer, out IntPtr ctmPointer)
                && WowInterface.XMemory.Read(IntPtr.Add(ctmPointer, WowInterface.OffsetList.ClickToMoveEnabled.ToInt32()), out int ctmEnabled))
            {
                if (ctmEnabled != 1)
                {
                    WowInterface.XMemory.Write(IntPtr.Add(ctmPointer, WowInterface.OffsetList.ClickToMoveEnabled.ToInt32()), 1);
                }
            }
        }

        public void FacePosition(WowPlayer player, Vector3 positionToFace)
        {
            float angle = BotMath.GetFacingAngle(player.Position, positionToFace);
            SetFacing(player, angle);
            // buggy atm
            // SendMovementPacket(player, 0xDA);
        }

        public void GameobjectOnRightClick(WowObject gameobject)
            => CallObjectFunction(gameobject.BaseAddress, WowInterface.OffsetList.FunctionGameobjectOnRightClick);

        public List<string> GetAuras(WowLuaUnit luaunit)
            => ReadAuras(luaunit, "UnitAura");

        public List<string> GetBuffs(WowLuaUnit luaunit)
            => ReadAuras(luaunit, "UnitBuff");

        public List<string> GetDebuffs(WowLuaUnit luaunit)
            => ReadAuras(luaunit, "UnitDebuff");

        public string GetEquipmentItems()
        {
            string command = "abotEquipmentResult=\"[\"for a=0,23 do abId=GetInventoryItemID(\"player\",a)if string.len(tostring(abId or\"\"))>0 then abotItemLink=GetInventoryItemLink(\"player\",a)abCount=GetInventoryItemCount(\"player\",a)abCurrentDurability,abMaxDurability=GetInventoryItemDurability(a)abCooldownStart,abCooldownEnd=GetInventoryItemCooldown(\"player\",a)abName,abLink,abRarity,abLevel,abMinLevel,abType,abSubType,abStackCount,abEquipLoc,abIcon,abSellPrice=GetItemInfo(abotItemLink)abstats=GetItemStats(abotItemLink)statsResult={}for b,c in pairs(abstats)do table.insert(statsResult,string.format(\"\\\"%s\\\":\\\"%s\\\"\",b,c))end;abotEquipmentResult=abotEquipmentResult..'{'..'\"id\": \"'..tostring(abId or 0)..'\",'..'\"count\": \"'..tostring(abCount or 0)..'\",'..'\"quality\": \"'..tostring(abRarity or 0)..'\",'..'\"curDurability\": \"'..tostring(abCurrentDurability or 0)..'\",'..'\"maxDurability\": \"'..tostring(abMaxDurability or 0)..'\",'..'\"cooldownStart\": \"'..tostring(abCooldownStart or 0)..'\",'..'\"cooldownEnd\": '..tostring(abCooldownEnd or 0)..','..'\"name\": \"'..tostring(abName or 0)..'\",'..'\"link\": \"'..tostring(abLink or 0)..'\",'..'\"level\": \"'..tostring(abLevel or 0)..'\",'..'\"minLevel\": \"'..tostring(abMinLevel or 0)..'\",'..'\"type\": \"'..tostring(abType or 0)..'\",'..'\"subtype\": \"'..tostring(abSubType or 0)..'\",'..'\"maxStack\": \"'..tostring(abStackCount or 0)..'\",'..'\"equipslot\": \"'..tostring(a or 0)..'\",'..'\"equiplocation\": \"'..tostring(abEquipLoc or 0)..'\",'..'\"stats\": '..\"{\"..table.concat(statsResult,\",\")..\"}\"..','..'\"sellprice\": \"'..tostring(abSellPrice or 0)..'\"'..'}'if a<23 then abotEquipmentResult=abotEquipmentResult..\",\"end end end;abotEquipmentResult=abotEquipmentResult..\"]\"";
            LuaDoString(command);
            return GetLocalizedText("abotEquipmentResult");
        }

        public int GetFreeBagSlotCount()
        {
            LuaDoString("abFreeBagSlots=0 for i=1,5 do abFreeBagSlots=abFreeBagSlots+GetContainerNumFreeSlots(i-1)end");

            if (int.TryParse(GetLocalizedText("abFreeBagSlots"), out int bagSlots))
            {
                return bagSlots;
            }
            else
            {
                return 100;
            }
        }

        public string GetInventoryItems()
        {
            string command = "abotInventoryResult=\"[\"for a=0,4 do containerSlots=GetContainerNumSlots(a)for b=1,containerSlots do abId=GetContainerItemID(a,b)if string.len(tostring(abId or\"\"))>0 then abItemLink=GetContainerItemLink(a,b)abCurrentDurability,abMaxDurability=GetContainerItemDurability(a,b)abCooldownStart,abCooldownEnd=GetContainerItemCooldown(a,b)abIcon,abItemCount,abLocked,abQuality,abReadable,abLootable,abItemLink,isFiltered=GetContainerItemInfo(a,b)abName,abLink,abRarity,abLevel,abMinLevel,abType,abSubType,abStackCount,abEquipLoc,abIcon,abSellPrice=GetItemInfo(abItemLink)abstats=GetItemStats(abItemLink)statsResult={}for c,d in pairs(abstats)do table.insert(statsResult,string.format(\"\\\"%s\\\":\\\"%s\\\"\",c,d))end;abotInventoryResult=abotInventoryResult..\"{\"..'\"id\": \"'..tostring(abId or 0)..'\",'..'\"count\": \"'..tostring(abItemCount or 0)..'\",'..'\"quality\": \"'..tostring(abRarity or 0)..'\",'..'\"curDurability\": \"'..tostring(abCurrentDurability or 0)..'\",'..'\"maxDurability\": \"'..tostring(abMaxDurability or 0)..'\",'..'\"cooldownStart\": \"'..tostring(abCooldownStart or 0)..'\",'..'\"cooldownEnd\": \"'..tostring(abCooldownEnd or 0)..'\",'..'\"name\": \"'..tostring(abName or 0)..'\",'..'\"lootable\": \"'..tostring(abLootable or 0)..'\",'..'\"readable\": \"'..tostring(abReadable or 0)..'\",'..'\"link\": \"'..tostring(abItemLink or 0)..'\",'..'\"level\": \"'..tostring(abLevel or 0)..'\",'..'\"minLevel\": \"'..tostring(abMinLevel or 0)..'\",'..'\"type\": \"'..tostring(abType or 0)..'\",'..'\"subtype\": \"'..tostring(abSubType or 0)..'\",'..'\"maxStack\": \"'..tostring(abStackCount or 0)..'\",'..'\"equiplocation\": \"'..tostring(abEquipLoc or 0)..'\",'..'\"sellprice\": \"'..tostring(abSellPrice or 0)..'\",'..'\"stats\": '..\"{\"..table.concat(statsResult,\",\")..\"}\"..','..'\"bagid\": \"'..tostring(a or 0)..'\",'..'\"bagslot\": \"'..tostring(b or 0)..'\"'..\"}\"abotInventoryResult=abotInventoryResult..\",\"end end end;abotInventoryResult=abotInventoryResult..\"]\"";
            LuaDoString(command);

            return GetLocalizedText("abotInventoryResult");
        }

        public string GetItemByNameOrLink(string itemName)
        {
            string command = $"abotItemName=\"{itemName}\";abotItemInfoResult='noItem';abName,abLink,abRarity,abLevel,abMinLevel,abType,abSubType,abStackCount,abEquipLoc,abIcon,abSellPrice=GetItemInfo(abotItemName);abotItemInfoResult='{{'..'\"id\": \"0\",'..'\"count\": \"1\",'..'\"quality\": \"'..tostring(abRarity or 0)..'\",'..'\"curDurability\": \"0\",'..'\"maxDurability\": \"0\",'..'\"cooldownStart\": \"0\",'..'\"cooldownEnd\": \"0\",'..'\"name\": \"'..tostring(abName or 0)..'\",'..'\"link\": \"'..tostring(abLink or 0)..'\",'..'\"level\": \"'..tostring(abLevel or 0)..'\",'..'\"minLevel\": \"'..tostring(abMinLevel or 0)..'\",'..'\"type\": \"'..tostring(abType or 0)..'\",'..'\"subtype\": \"'..tostring(abSubType or 0)..'\",'..'\"maxStack\": \"'..tostring(abStackCount or 0)..'\",'..'\"equiplocation\": \"'..tostring(abEquipLoc or 0)..'\",'..'\"sellprice\": \"'..tostring(abSellPrice or 0)..'\"'..'}}';";

            LuaDoString(command);
            return GetLocalizedText("abotItemInfoResult");
        }

        public string GetItemBySlot(int itemslot)
        {
            string command = $"abotItemSlot={itemslot};abotItemInfoResult='noItem';abId=GetInventoryItemID('player',abotItemSlot);abCount=GetInventoryItemCount('player',abotItemSlot);abQuality=GetInventoryItemQuality('player',abotItemSlot);abCurrentDurability,abMaxDurability=GetInventoryItemDurability(abotItemSlot);abCooldownStart,abCooldownEnd=GetInventoryItemCooldown('player',abotItemSlot);abName,abLink,abRarity,abLevel,abMinLevel,abType,abSubType,abStackCount,abEquipLoc,abIcon,abSellPrice=GetItemInfo(GetInventoryItemLink('player',abotItemSlot));abotItemInfoResult='{{'..'\"id\": \"'..tostring(abId or 0)..'\",'..'\"count\": \"'..tostring(abCount or 0)..'\",'..'\"quality\": \"'..tostring(abQuality or 0)..'\",'..'\"curDurability\": \"'..tostring(abCurrentDurability or 0)..'\",'..'\"maxDurability\": \"'..tostring(abMaxDurability or 0)..'\",'..'\"cooldownStart\": \"'..tostring(abCooldownStart or 0)..'\",'..'\"cooldownEnd\": '..tostring(abCooldownEnd or 0)..','..'\"name\": \"'..tostring(abName or 0)..'\",'..'\"link\": \"'..tostring(abLink or 0)..'\",'..'\"level\": \"'..tostring(abLevel or 0)..'\",'..'\"minLevel\": \"'..tostring(abMinLevel or 0)..'\",'..'\"type\": \"'..tostring(abType or 0)..'\",'..'\"subtype\": \"'..tostring(abSubType or 0)..'\",'..'\"maxStack\": \"'..tostring(abStackCount or 0)..'\",'..'\"equipslot\": \"'..tostring(abEquipLoc or 0)..'\",'..'\"sellprice\": \"'..tostring(abSellPrice or 0)..'\"'..'}}';";

            LuaDoString(command);
            return GetLocalizedText("abotItemInfoResult");
        }

        public string GetItemStats(string itemLink)
        {
            string command = $"abotItemLink=\"{itemLink}\"abotItemStatsResult=''stats={{}}abStats=GetItemStats(abotItemLink,stats)abotItemStatsResult='{{'..'\"stamina\": \"'..tostring(stats[\"ITEM_MOD_STAMINA_SHORT\"]or 0)..'\",'..'\"agility\": \"'..tostring(stats[\"ITEM_MOD_AGILITY_SHORT\"]or 0)..'\",'..'\"strenght\": \"'..tostring(stats[\"ITEM_MOD_STRENGHT_SHORT\"]or 0)..'\",'..'\"intellect\": \"'..tostring(stats[\"ITEM_MOD_INTELLECT_SHORT\"]or 0)..'\",'..'\"spirit\": \"'..tostring(stats[\"ITEM_MOD_SPIRIT_SHORT\"]or 0)..'\",'..'\"attackpower\": \"'..tostring(stats[\"ITEM_MOD_ATTACK_POWER_SHORT\"]or 0)..'\",'..'\"spellpower\": \"'..tostring(stats[\"ITEM_MOD_SPELL_POWER_SHORT\"]or 0)..'\",'..'\"mana\": \"'..tostring(stats[\"ITEM_MOD_MANA_SHORT\"]or 0)..'\"'..'}}'";

            LuaDoString(command);
            return GetLocalizedText("abotItemStatsResult");
        }

        public string GetLocalizedText(string variable)
        {
            AmeisenLogger.Instance.Log("HookManager", $"GetLocalizedText: {variable}...", LogLevel.Verbose);

            if (variable.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(variable);
                if (WowInterface.XMemory.AllocateMemory((uint)bytes.Length + 1, out IntPtr memAlloc))
                {
                    WowInterface.XMemory.WriteBytes(memAlloc, bytes);

                    if (memAlloc == IntPtr.Zero)
                    {
                        return string.Empty;
                    }

                    string[] asm = new string[]
                    {
                        $"CALL {WowInterface.OffsetList.FunctionGetActivePlayerObject.ToInt32()}",
                        "MOV ECX, EAX",
                        "PUSH -1",
                        $"PUSH {memAlloc.ToInt32()}",
                        $"CALL {WowInterface.OffsetList.FunctionGetLocalizedText.ToInt32()}",
                        "RETN",
                    };

                    string result = Encoding.UTF8.GetString(InjectAndExecute(asm, true));
                    WowInterface.XMemory.FreeMemory(memAlloc);
                    return result;
                }
            }

            return string.Empty;
        }

        public string GetLootRollItemLink(int rollId)
        {
            LuaDoString($"abRollItemLink = GetLootRollItemLink({rollId});");
            return GetLocalizedText("abRollItemLink");
        }

        public string GetMoney()
        {
            LuaDoString("abMoney = GetMoney();");
            return GetLocalizedText("abMoney");
        }

        public Dictionary<RuneType, int> GetRunesReady(int runeId)
        {
            Dictionary<RuneType, int> runes = new Dictionary<RuneType, int>()
            {
                { RuneType.Blood, 0 },
                { RuneType.Frost, 0 },
                { RuneType.Unholy, 0 },
                { RuneType.Death, 0 }
            };

            for (int i = 0; i < 6; ++i)
            {
                if (WowInterface.XMemory.Read(WowInterface.OffsetList.RuneType + (4 * i), out RuneType type)
                    && WowInterface.XMemory.ReadByte(WowInterface.OffsetList.Runes, out byte runeStatus)
                    && ((1 << runeId) & runeStatus) != 0)
                {
                    runes[type]++;
                }
            }

            return runes;
        }

        public List<string> GetSkills()
        {
            LuaDoString("abSkillList=\"\"abSkillCount=GetNumSkillLines()for a=1,abSkillCount do local b,c=GetSkillLineInfo(a)if not c then abSkillList=abSkillList..b;if a<abSkillCount then abSkillList=abSkillList..\"; \"end end end");

            try
            {
                return new List<string>(GetLocalizedText("abSkillList").Split(';'));
            }
            catch
            {
                return new List<string>();
            }
        }

        public double GetSpellCooldown(string spellName)
        {
            LuaDoString($"start,duration,enabled = GetSpellCooldown(\"{spellName}\");cdLeft = (start + duration - GetTime());");
            string result = GetLocalizedText("cdLeft").Replace(".", ",");

            if (double.TryParse(result, out double value))
            {
                value = Math.Round(value, 3);
                return value > 0 ? value * 1000 : 0;
            }

            return -1;
        }

        public string GetSpellNameById(int spellId)
        {
            LuaDoString($"abotSpellName=GetSpellInfo({spellId});");
            return GetLocalizedText("abotSpellName");
        }

        public string GetSpells()
        {
            string command = "abotSpellResult='['tabCount=GetNumSpellTabs()for a=1,tabCount do tabName,tabTexture,tabOffset,numEntries=GetSpellTabInfo(a)for b=tabOffset+1,tabOffset+numEntries do abSpellName,abSpellRank=GetSpellName(b,\"BOOKTYPE_SPELL\")if abSpellName then abName,abRank,_,abCosts,_,_,abCastTime,abMinRange,abMaxRange=GetSpellInfo(abSpellName,abSpellRank)abotSpellResult=abotSpellResult..'{'..'\"spellbookName\": \"'..tostring(tabName or 0)..'\",'..'\"spellbookId\": \"'..tostring(a or 0)..'\",'..'\"name\": \"'..tostring(abSpellName or 0)..'\",'..'\"rank\": \"'..tostring(abRank or 0)..'\",'..'\"castTime\": \"'..tostring(abCastTime or 0)..'\",'..'\"minRange\": \"'..tostring(abMinRange or 0)..'\",'..'\"maxRange\": \"'..tostring(abMaxRange or 0)..'\",'..'\"costs\": \"'..tostring(abCosts or 0)..'\"'..'}'if a<tabCount or b<tabOffset+numEntries then abotSpellResult=abotSpellResult..','end end end end;abotSpellResult=abotSpellResult..']'";
            LuaDoString(command);
            return GetLocalizedText("abotSpellResult");
        }

        public List<WowAura> GetUnitAuras(IntPtr baseAddress)
        {
            List<WowAura> buffs = new List<WowAura>();
            if (WowInterface.XMemory.Read(baseAddress + 0xC30, out int buffTable))
            {
                IntPtr buffBase;
                if (buffTable == 0)
                {
                    buffBase = IntPtr.Add(baseAddress, 0xC38);
                }
                else
                {
                    buffBase = IntPtr.Add(baseAddress, 0xC30);
                }

                int count = 1;

                do
                {
                    if (WowInterface.XMemory.Read(IntPtr.Add(buffBase, 0x18 * count), out RawWowAura aura))
                    {
                        if (aura.SpellId > 0)
                        {
                            if (!WowInterface.BotCache.TryGetSpellName(aura.SpellId, out string name))
                            {
                                name = GetSpellNameById(aura.SpellId);
                                WowInterface.BotCache.CacheSpellName(aura.SpellId, name);
                            }

                            if (name.Length > 0 && !buffs.Any(e => e.Name == name))
                            {
                                buffs.Add(new WowAura(aura, name));
                            }
                        }

                        count++;
                    }
                    else
                    {
                        break;
                    }
                } while (count < 32);
            }

            return buffs;
        }

        /// <summary>
        /// Check if the WowLuaUnit is casting or channeling a spell
        /// </summary>
        /// <param name="luaunit">player, target, party1...</param>
        /// <returns>(Spellname, duration)</returns>
        public (string, int) GetUnitCastingInfo(WowLuaUnit luaunit)
        {
            string command = $"abCastingInfo = \"none,0\"; abSpellName, x, x, x, x, abSpellEndTime = UnitCastingInfo(\"{luaunit.ToString()}\"); abDuration = ((abSpellEndTime/1000) - GetTime()) * 1000; abCastingInfo = abSpellName..\",\"..abDuration;";
            LuaDoString(command);

            string str = GetLocalizedText("abCastingInfo");
            if (double.TryParse(str.Split(',')[1], out double timeRemaining))
            {
                return (str.Split(',')[0], (int)Math.Round(timeRemaining, 0));
            }

            return (string.Empty, 0);
        }

        public WowUnitReaction GetUnitReaction(WowUnit wowUnitA, WowUnit wowUnitB)
        {
            WowUnitReaction reaction = WowUnitReaction.Unknown;

            if (wowUnitA == null || wowUnitB == null)
            {
                return reaction;
            }

            if (WowInterface.BotCache.TryGetReaction(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate, out WowUnitReaction cachedReaction))
            {
                return cachedReaction;
            }

            // integer to save the reaction
            WowInterface.XMemory.AllocateMemory(4, out IntPtr memAlloc);
            WowInterface.XMemory.Write(memAlloc, 0);

            // TODO: refactor this
            string[] asm = new string[]
            {
                $"PUSH {wowUnitA.BaseAddress}",
                $"MOV ECX, {wowUnitB.BaseAddress}",
                $"CALL {WowInterface.OffsetList.FunctionUnitGetReaction}",
                $"MOV [{memAlloc}], EAX",
                "RETN",
            };

            if (wowUnitA.IsDead || wowUnitB.IsDead)
            {
                return reaction;
            }

            try
            {
                InjectAndExecute(asm, true);
                WowInterface.XMemory.Read(memAlloc, out reaction);

                WowInterface.BotCache.CacheReaction(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate, reaction);
            }
            finally
            {
                WowInterface.XMemory.FreeMemory(memAlloc);
            }

            return reaction;
        }

        public bool HasUnitStealableBuffs(WowLuaUnit luaUnit)
        {
            LuaDoString($"abIsStealableStuffThere=0;local y=0;for i=1,40 do local n,_,_,_,_,_,_,_,isStealable=UnitAura(\"{luaUnit}\",i);if isStealable==1 then abIsStealableStuffThere=1;end end");
            string rawValue = GetLocalizedText("abIsStealableStuffThere");

            return int.TryParse(rawValue, out int result) ? result == 1 : false;
        }

        public byte[] InjectAndExecute(string[] asm, bool readReturnBytes)
        {
            AmeisenLogger.Instance.Log("HookManager", $"Injecting: {JsonConvert.SerializeObject(asm)}...", LogLevel.Verbose);

            List<byte> returnBytes = new List<byte>();

            if (!WowInterface.ObjectManager.IsWorldLoaded)
            {
                return returnBytes.ToArray();
            }

            try
            {
                int timeoutCounter = 0;

                // wait for the code to be executed
                while (IsInjectionUsed)
                {
                    if (timeoutCounter == 5000)
                    {
                        return Array.Empty<byte>();
                    }

                    timeoutCounter++;
                    Thread.Sleep(1);
                }

                IsInjectionUsed = true;

                // preparing to inject the given ASM
                WowInterface.XMemory.Fasm.Clear();

                // add all lines
                foreach (string s in asm)
                {
                    WowInterface.XMemory.Fasm.AddLine(s);
                }

                // inject it
                WowInterface.XMemory.SuspendMainThread();
                WowInterface.XMemory.Fasm.Inject((uint)CodecaveForExecution.ToInt32());
                WowInterface.XMemory.ResumeMainThread();

                // now there is code to be executed
                WowInterface.XMemory.Write(CodeToExecuteAddress, 1);

                timeoutCounter = 0;

                // wait for the code to be executed
                while (WowInterface.XMemory.Read(CodeToExecuteAddress, out int codeToBeExecuted) && codeToBeExecuted > 0)
                {
                    if (timeoutCounter == 5000)
                    {
                        return Array.Empty<byte>();
                    }

                    timeoutCounter++;
                    IsInjectionUsed = false;
                    Thread.Sleep(1);
                }

                // if we want to read the return value do it otherwise we're done
                if (readReturnBytes)
                {
                    try
                    {
                        WowInterface.XMemory.Read(ReturnValueAddress, out IntPtr dwAddress);

                        // read all parameter-bytes until we the buffer is 0
                        WowInterface.XMemory.ReadByte(dwAddress, out byte buffer);
                        while (buffer != 0)
                        {
                            returnBytes.Add(buffer);
                            dwAddress = IntPtr.Add(dwAddress, 1);
                            WowInterface.XMemory.ReadByte(dwAddress, out buffer);
                        }
                    }
                    catch (Exception e)
                    {
                        AmeisenLogger.Instance.Log("HookManager", $"Failed to read return bytes:\n{e.ToString()}", LogLevel.Error);
                    }
                }

                IsInjectionUsed = false;
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log("HookManager", $"Failed to inject:\n{e.ToString()}", LogLevel.Error);

                // now there is no more code to be executed
                WowInterface.XMemory.Write(CodeToExecuteAddress, 0);
                IsInjectionUsed = false;
                WowInterface.XMemory.ResumeMainThread();
            }

            return returnBytes.ToArray();
        }

        public bool IsBgInviteReady()
        {
            LuaDoString("abBgQueueIsReady = 0;for i=1,2 do local x = GetBattlefieldPortExpiration(i) if x > 0 then abBgQueueIsReady = 1 end end");
            string rawValue = GetLocalizedText("abBgQueueIsReady");

            return int.TryParse(rawValue, out int result) ? result == 1 : false;
        }

        public bool IsClickToMovePending()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.ClickToMovePendingMovement, out byte ctmPending))
            {
                return ctmPending == 2;
            }

            return false;
        }

        public bool IsGhost(string unit)
        {
            LuaDoString($"isGhost = UnitIsGhost(\"{unit}\");");
            string result = GetLocalizedText("isGhost");

            if (int.TryParse(result, out int isGhost))
            {
                return isGhost == 1;
            }

            return false;
        }

        public bool IsRuneReady(int runeId)
        {
            if (WowInterface.XMemory.ReadByte(WowInterface.OffsetList.Runes, out byte runeStatus))
            {
                return ((1 << runeId) & runeStatus) != 0;
            }
            else
            {
                return false;
            }
        }

        public bool IsSpellKnown(int spellId, bool isPetSpell = false)
        {
            LuaDoString($"abIsSpellKnown = IsSpellKnown({spellId}, {isPetSpell});");
            string rawValue = GetLocalizedText("abIsSpellKnown");

            return bool.TryParse(rawValue, out bool result) ? result : false;
        }

        public void KickNpcsOutOfMammoth()
            => LuaDoString("for i = 1, 2 do EjectPassengerFromSeat(i) end");

        public void LearnAllAvaiableSpells()
            => LuaDoString("/run LoadAddOn\"Blizzard_TrainerUI\" f=ClassTrainerTrainButton f.e = 0 if f:GetScript\"OnUpdate\" then f:SetScript(\"OnUpdate\", nil)else f:SetScript(\"OnUpdate\", function(f,e) f.e=f.e+e if f.e>.01 then f.e=0 f:Click() end end)end");

        public void LeaveBattleground()
            => SendChatMessage("/click WorldStateScoreFrameLeaveButton");

        public void LootEveryThing()
            => LuaDoString("abLootCount=GetNumLootItems();for i = abLootCount,1,-1 do LootSlot(i); ConfirmLootSlot(i); end");

        public void LuaDoString(string command)
        {
            AmeisenLogger.Instance.Log("HookManager", $"LuaDoString: {command}...", LogLevel.Verbose);

            if (command.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(command);
                if (WowInterface.XMemory.AllocateMemory((uint)bytes.Length + 1, out IntPtr memAlloc))
                {
                    WowInterface.XMemory.WriteBytes(memAlloc, bytes);

                    if (memAlloc == IntPtr.Zero)
                    {
                        return;
                    }

                    string[] asm = new string[]
                    {
                        $"MOV EAX, {memAlloc.ToInt32()}",
                        "PUSH 0",
                        "PUSH EAX",
                        "PUSH EAX",
                        $"CALL {WowInterface.OffsetList.FunctionLuaDoString.ToInt32()}",
                        "ADD ESP, 0xC",
                        "RETN",
                    };

                    InjectAndExecute(asm, false);
                    WowInterface.XMemory.FreeMemory(memAlloc);
                }
            }
        }

        public void QueueBattlegroundByName(string bgName)
            => SendChatMessage($"/run for i=1,GetNumBattlegroundTypes()do local name,_,_,_,_=GetBattlegroundInfo(i)if name==\"{bgName}\"then JoinBattlefield(i)end end");

        public void ReleaseSpirit()
            => LuaDoString("RepopMe();");

        public void RepairAllItems()
            => LuaDoString("RepairAllItems();");

        public void ReplaceItem(IWowItem currentItem, IWowItem newItem)
        {
            if (currentItem == null)
            {
                LuaDoString($"EquipItemByName(\"{newItem.Name}\");");
            }
            else
            {
                LuaDoString($"EquipItemByName(\"{newItem.Name}\", {(int)currentItem.EquipSlot});");
            }

            CofirmBop();
        }

        public void RetrieveCorpse()
            => LuaDoString("RetrieveCorpse();");

        /// <summary>
        /// Roll something on a dropped item
        /// </summary>
        /// <param name="rollId">The rolls id to roll on</param>
        /// <param name="rollType">Need, Greed or Pass</param>
        public void RollOnItem(int rollId, RollType rollType)
        {
            LuaDoString($"RollOnLoot({rollId}, {(int)rollType});");
            SendChatMessage("/click StaticPopup1Button1");
        }

        public void SellAllGrayItems()
            => LuaDoString("local p,N,n=0 for b=0,4 do for s=1,GetContainerNumSlots(b) do n=GetContainerItemLink(b,s) if n and string.find(n,\"9d9d9d\") then N={GetItemInfo(n)} p=p+N[11] UseContainerItem(b,s) end end end");

        public void SellAllItems()
            => LuaDoString("local p,N,n=0 for b=0,4 do for s=1,GetContainerNumSlots(b) do n=GetContainerItemLink(b,s) if n then N={GetItemInfo(n)} p=p+N[11] UseContainerItem(b,s) end end end");

        public void SellItemsByName(string itemName)
            => LuaDoString($"for bag = 0,4,1 do for slot = 1, GetContainerNumSlots(bag), 1 do local name = GetContainerItemLink(bag,slot); if name and string.find(name,\"{itemName}\") then UseContainerItem(bag,slot) end end end");

        public void SendChatMessage(string message)
            => LuaDoString($"DEFAULT_CHAT_FRAME.editBox:SetText(\"{message}\") ChatEdit_SendText(DEFAULT_CHAT_FRAME.editBox, 0)");

        public void SendItemMailToCharacter(string itemName, string receiver)
        {
            SendChatMessage($"/run for b=0,4 do for s=0,36 do I=GetContainerItemLink(b,s) if I and I:find(\"{itemName}\")then UseContainerItem(b,s) end end end SendMailNameEditBox:SetText(\"{receiver}\"))");
            SendChatMessage($"/run SendMailMailButton:Click()");
        }

        public void SendMovementPacket(WowUnit unit, int opcode)
            => CallObjectFunction(unit.BaseAddress, WowInterface.OffsetList.FunctionUnitSendMovementPacket, new List<object>() { opcode, Environment.TickCount });

        public void SetFacing(WowUnit unit, float angle)
            => CallObjectFunction(unit.BaseAddress, WowInterface.OffsetList.FunctionUnitSetFacing, new List<object>() { angle.ToString().Replace(',', '.'), Environment.TickCount });

        public void SetMaxFps(byte maxFps) => WowInterface.XMemory.Write(WowInterface.OffsetList.CvarMaxFps, maxFps);

        public bool SetupEndsceneHook()
        {
            AmeisenLogger.Instance.Log("HookManager", "Setting up the EndsceneHook...", LogLevel.Verbose);
            EndsceneAddress = GetEndScene();

            // first thing thats 5 bytes big is here
            // we are going to replace this 5 bytes with
            // our JMP instruction (JMP (1 byte) + Address (4 byte))
            EndsceneAddress = IntPtr.Add(EndsceneAddress, ENDSCENE_HOOK_OFFSET);
            EndsceneReturnAddress = IntPtr.Add(EndsceneAddress, 0x5);

            AmeisenLogger.Instance.Log("HookManager", $"Endscene is at: {EndsceneAddress.ToString("X")}", LogLevel.Verbose);

            // if WoW is already hooked, unhook it
            if (IsWoWHooked)
            {
                DisposeHook();
            }
            else
            {
                if (WowInterface.XMemory.ReadBytes(EndsceneAddress, 5, out byte[] bytes))
                {
                    OriginalEndsceneBytes = bytes;
                }

                if (!AllocateCodeCaves())
                {
                    return false;
                }

                WowInterface.XMemory.Fasm.Clear();

                // save registers
                WowInterface.XMemory.Fasm.AddLine("PUSHFD");
                WowInterface.XMemory.Fasm.AddLine("PUSHAD");

                // check for code to be executed
                WowInterface.XMemory.Fasm.AddLine($"MOV EBX, [{CodeToExecuteAddress.ToInt32()}]");
                WowInterface.XMemory.Fasm.AddLine("TEST EBX, 1");
                WowInterface.XMemory.Fasm.AddLine("JE @out");

                // set register back to zero
                WowInterface.XMemory.Fasm.AddLine("XOR EBX, EBX");

                // check for world to be loaded
                // we dont want to execute code in
                // the loadingscreen, cause that
                // mostly results in crashes
                WowInterface.XMemory.Fasm.AddLine($"MOV EBX, [{WowInterface.OffsetList.IsWorldLoaded.ToInt32()}]");
                WowInterface.XMemory.Fasm.AddLine("TEST EBX, 1");
                WowInterface.XMemory.Fasm.AddLine("JE @out");

                // execute our stuff and get return address
                WowInterface.XMemory.Fasm.AddLine($"MOV EDX, {CodecaveForExecution.ToInt32()}");
                WowInterface.XMemory.Fasm.AddLine("CALL EDX");
                WowInterface.XMemory.Fasm.AddLine($"MOV [{ReturnValueAddress.ToInt32()}], EAX");

                // finish up our execution
                WowInterface.XMemory.Fasm.AddLine("@out:");
                WowInterface.XMemory.Fasm.AddLine("MOV EDX, 0");
                WowInterface.XMemory.Fasm.AddLine($"MOV [{CodeToExecuteAddress.ToInt32()}], EDX");

                // restore registers
                WowInterface.XMemory.Fasm.AddLine("POPAD");
                WowInterface.XMemory.Fasm.AddLine("POPFD");

                byte[] asmBytes = WowInterface.XMemory.Fasm.Assemble();

                // needed to determine the position where the original
                // asm is going to be placed
                int asmLenght = asmBytes.Length;

                // inject the instructions into our codecave
                WowInterface.XMemory.Fasm.Inject((uint)CodecaveForCheck.ToInt32());

                // ---------------------------------------------------
                // End of the code that checks if there is asm to be
                // executed on our hook
                // ---------------------------------------------------

                // Prepare to replace the instructions inside WoW
                WowInterface.XMemory.Fasm.Clear();

                // do the original EndScene stuff after we restored the registers
                // and insert it after our code
                WowInterface.XMemory.WriteBytes(IntPtr.Add(CodecaveForCheck, asmLenght), OriginalEndsceneBytes);
                AmeisenLogger.Instance.Log("HookManager", $"EndsceneHook OriginalEndsceneBytes: {JsonConvert.SerializeObject(OriginalEndsceneBytes)}", LogLevel.Verbose);

                // return to original function after we're done with our stuff
                WowInterface.XMemory.Fasm.AddLine($"JMP {EndsceneReturnAddress.ToInt32()}");
                WowInterface.XMemory.Fasm.Inject((uint)CodecaveForCheck.ToInt32() + (uint)asmLenght + 5);
                WowInterface.XMemory.Fasm.Clear();

                // ---------------------------------------------------
                // End of doing the original stuff and returning to
                // the original instruction
                // ---------------------------------------------------

                // modify original EndScene instructions to start the hook
                WowInterface.XMemory.Fasm.AddLine($"JMP {CodecaveForCheck.ToInt32()}");
                WowInterface.XMemory.Fasm.Inject((uint)EndsceneAddress.ToInt32());

                AmeisenLogger.Instance.Log("HookManager", "EndsceneHook Successful...", LogLevel.Verbose);

                // we should've hooked WoW now
                return true;
            }

            return false;
        }

        public void StartAutoAttack() => SendChatMessage("/startattack");

        public void StopClickToMove(WowPlayer player) => CallObjectFunction(player.BaseAddress, WowInterface.OffsetList.FunctionPlayerClickToMoveStop);

        public void TargetGuid(ulong guid)
        {
            byte[] guidBytes = BitConverter.GetBytes(guid);
            string[] asm = new string[]
            {
                $"PUSH {BitConverter.ToUInt32(guidBytes, 4)}",
                $"PUSH {BitConverter.ToUInt32(guidBytes, 0)}",
                $"CALL {WowInterface.OffsetList.FunctionSetTarget.ToInt32()}",
                "ADD ESP, 0x8",
                "RETN"
            };

            InjectAndExecute(asm, false);
        }

        public void TargetLuaUnit(WowLuaUnit unit)
            => LuaDoString($"TargetUnit(\"{unit.ToString()}\");");

        public void TargetNearestEnemy()
            => SendChatMessage("/targetenemy [harm][nodead]");

        public void UnitOnRightClick(WowUnit unit)
            => CallObjectFunction(unit.BaseAddress, WowInterface.OffsetList.FunctionUnitOnRightClick);

        public void UseItemByBagAndSlot(int bagId, int bagSlot)
            => LuaDoString($"UseContainerItem({bagId}, {bagSlot});");

        public void UseItemByName(string itemName)
            => SellItemsByName(itemName);

        private bool AllocateCodeCaves()
        {
            AmeisenLogger.Instance.Log("HookManager", "Allocating Codecaves for the EndsceneHook...", LogLevel.Verbose);

            // integer to check if there is code waiting to be executed
            if (!WowInterface.XMemory.AllocateMemory(4, out IntPtr codeToExecuteAddress))
            {
                return false;
            }

            CodeToExecuteAddress = codeToExecuteAddress;
            WowInterface.XMemory.Write(CodeToExecuteAddress, 0);
            AmeisenLogger.Instance.Log("HookManager", $"EndsceneHook CodeToExecuteAddress: {codeToExecuteAddress.ToString("X")}", LogLevel.Verbose);

            // integer to save the pointer to the return value
            if (!WowInterface.XMemory.AllocateMemory(4, out IntPtr returnValueAddress))
            {
                return false;
            }

            ReturnValueAddress = returnValueAddress;
            WowInterface.XMemory.Write(ReturnValueAddress, 0);
            AmeisenLogger.Instance.Log("HookManager", $"EndsceneHook ReturnValueAddress: {returnValueAddress.ToString("X")}", LogLevel.Verbose);

            // codecave to check wether we need to execute something
            if (!WowInterface.XMemory.AllocateMemory(128, out IntPtr codecaveForCheck))
            {
                return false;
            }

            CodecaveForCheck = codecaveForCheck;
            AmeisenLogger.Instance.Log("HookManager", $"EndsceneHook CodecaveForCheck: {codecaveForCheck.ToString("X")}", LogLevel.Verbose);

            // codecave for the code we wan't to execute
            if (!WowInterface.XMemory.AllocateMemory(2048, out IntPtr codecaveForExecution))
            {
                return false;
            }

            CodecaveForExecution = codecaveForExecution;
            AmeisenLogger.Instance.Log("HookManager", $"EndsceneHook CodecaveForExecution: {codecaveForExecution.ToString("X")}", LogLevel.Verbose);

            return true;
        }

        private byte[] CallObjectFunction(IntPtr objectBaseAddress, IntPtr functionAddress, List<object> args = null, bool readReturnBytes = false)
        {
            List<string> asm = new List<string>();

            if (args != null)
            {
                foreach (object arg in args)
                {
                    asm.Add($"PUSH {arg}");
                }
            }

            asm.Add($"MOV ECX, {objectBaseAddress}");
            asm.Add($"CALL {functionAddress}");
            asm.Add("RETN");

            return InjectAndExecute(asm.ToArray(), readReturnBytes);
        }

        private IntPtr GetEndScene()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.EndSceneStaticDevice, out IntPtr pDevice)
                && WowInterface.XMemory.Read(IntPtr.Add(pDevice, WowInterface.OffsetList.EndSceneOffsetDevice.ToInt32()), out IntPtr pEnd)
                && WowInterface.XMemory.Read(pEnd, out IntPtr pScene)
                && WowInterface.XMemory.Read(IntPtr.Add(pScene, WowInterface.OffsetList.EndSceneOffset.ToInt32()), out IntPtr pEndscene))
            {
                return pEndscene;
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        private List<string> ReadAuras(WowLuaUnit luaunit, string functionName)
        {
            string command = $"local a,b={{}},1;local c={functionName}(\"{luaunit.ToString()}\",b)while c do a[#a+1]=c;b=b+1;c={functionName}(\"{luaunit.ToString()}\",b)end;if#a<1 then a=\"\"else activeAuras=table.concat(a,\",\")end";

            LuaDoString(command);
            string[] debuffs = GetLocalizedText("activeAuras").Split(',');

            List<string> resultLowered = new List<string>();
            foreach (string s in debuffs)
            {
                resultLowered.Add(s.Trim().ToLower());
            }

            return resultLowered;
        }
    }
}