using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Data.Persistence;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Pathfinding.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace AmeisenBotX.Core.Hook
{
    public class HookManager
    {
        private const int ENDSCENE_HOOK_OFFSET = 0x2;

        public HookManager(XMemory xMemory, IOffsetList offsetList, ObjectManager objectManager, IAmeisenBotCache botCache)
        {
            XMemory = xMemory;
            OffsetList = offsetList;
            ObjectManager = objectManager;
            BotCache = botCache;
        }

        public byte[] OriginalEndsceneBytes { get; private set; }

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
                if (XMemory.ReadBytes(EndsceneAddress, 1, out byte[] c))
                {
                    return c[0] == 0xE9;
                }
                else
                {
                    return false;
                }
            }
        }

        public void SetMaxFps(byte maxFps)
        {
            XMemory.Write(OffsetList.CvarMaxFps, maxFps);
        }

        public bool IsClickToMovePending()
        {
            if (XMemory.Read(OffsetList.ClickToMovePendingMovement, out byte ctmPending))
            {
                return ctmPending == 2;
            }

            return false;
        }

        public void ClickOnTerrain(Vector3 position)
        {
            if (XMemory.AllocateMemory(20, out IntPtr codeCaveVector3))
            {
                XMemory.Write<ulong>(codeCaveVector3, 0);
                XMemory.Write(IntPtr.Add(codeCaveVector3, 0x8), position);

                string[] asm = new string[]
                {
                    $"PUSH {codeCaveVector3.ToInt32()}",
                    $"CALL 0x{OffsetList.FunctionHandleTerrainClick.ToString("X")}",
                    "ADD ESP, 0x4",
                    "RETN",
                };

                InjectAndExecute(asm, false);
                XMemory.FreeMemory(codeCaveVector3);
            }
        }

        public void ClickToMove(IntPtr playerBase, Vector3 position)
        {
            if (XMemory.AllocateMemory(12, out IntPtr codeCaveVector3))
            {
                XMemory.Write(codeCaveVector3, position);

                string[] asm = new string[]
                {
                    $"PUSH {codeCaveVector3.ToInt32()}",
                    $"MOV ECX, {playerBase.ToInt32()}",
                    $"CALL 0x{OffsetList.FunctionClickToMove.ToString("X")}",
                    "RETN",
                };

                InjectAndExecute(asm, false);
                XMemory.FreeMemory(codeCaveVector3);
            }
        }

        public IntPtr ReturnValueAddress { get; private set; }

        private IAmeisenBotCache BotCache { get; }

        private ObjectManager ObjectManager { get; }

        private IOffsetList OffsetList { get; }

        private XMemory XMemory { get; }

        public string GetMoney()
        {
            LuaDoString("abMoney = GetMoney();");
            return GetLocalizedText("abMoney");
        }

        public void AcceptPartyInvite()
        {
            LuaDoString("AcceptGroup();");
            SendChatMessage("/click StaticPopup1Button1");
        }

        public void AcceptResurrect()
        {
            LuaDoString("AcceptResurrect();");
            SendChatMessage("/click StaticPopup1Button1");
        }

        public void AcceptSummon()
        {
            LuaDoString("ConfirmSummon();");
            SendChatMessage("/click StaticPopup1Button1");
        }

        public void AttackUnit(WowUnit unit)
        {
            XMemory.Write(OffsetList.ClickToMoveGuid, unit.Guid);
            WriteCtmValues(unit.Position, ClickToMoveType.AttackGuid);
        }

        public void CastSpell(int spellId)
            => LuaDoString($"CastSpell({spellId});");

        public void CastSpell(string name, bool castOnSelf = false)
        {
            AmeisenLogger.Instance.Log($"Casting spell with name: {name}", LogLevel.Verbose);

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
            AmeisenLogger.Instance.Log($"Casting spell with id: {spellId}", LogLevel.Verbose);

            if (spellId > 0)
            {
                string[] asmLocalText = new string[]
                {
                    "PUSH 0",
                    "PUSH 0",
                    "PUSH 0",
                    $"PUSH {spellId}",
                    $"CALL 0x{OffsetList.FunctionCastSpellById.ToString("X")}",
                    "ADD ESP, 0x10",
                };

                InjectAndExecute(asmLocalText, false);
            }
        }

        public void CofirmBop()
        {
            LuaDoString("ConfirmBindOnUse();");
            SendChatMessage("/click StaticPopup1Button1");
        }

        public void CofirmReadyCheck(bool isReady)
        {
            LuaDoString($"ConfirmReadyCheck({isReady});");
        }

        public void DisposeHook()
        {
            if (IsWoWHooked)
            {
                AmeisenLogger.Instance.Log("Disposing EnsceneHook...", LogLevel.Verbose);
                XMemory.WriteBytes(EndsceneAddress, OriginalEndsceneBytes);

                if (CodecaveForCheck != null)
                {
                    XMemory.FreeMemory(CodecaveForCheck);
                }

                if (CodecaveForExecution != null)
                {
                    XMemory.FreeMemory(CodecaveForExecution);
                }

                if (CodeToExecuteAddress != null)
                {
                    XMemory.FreeMemory(CodeToExecuteAddress);
                }

                if (ReturnValueAddress != null)
                {
                    XMemory.FreeMemory(ReturnValueAddress);
                }
            }
        }

        public bool IsRuneReady(int runeId)
        {
            if (XMemory.ReadByte(OffsetList.Runes, out byte runeStatus))
            {
                return ((1 << runeId) & runeStatus) != 0;
            }
            else
            {
                return false;
            }
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

        public bool IsSpellKnown(int spellId, bool isPetSpell = false)
        {
            LuaDoString($"abIsSpellKnown = IsSpellKnown({spellId}, {isPetSpell});");
            string rawValue = GetLocalizedText("abIsSpellKnown");

            return bool.TryParse(rawValue, out bool result) ? result : false;
        }

        public void FaceUnit(WowPlayer player, Vector3 positionToFace)
        {
            float angle = BotMath.GetFacingAngle(player.Position, positionToFace);
            XMemory.Write(IntPtr.Add(player.BaseAddress, OffsetList.PlayerRotation.ToInt32()), angle);
            BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr(0x41), 0, 0); // the "S" key to go a bit backwards TODO: find better method 0x53
        }

        public List<string> GetAuras(string luaunitName)
        {
            List<string> result = new List<string>(GetBuffs(luaunitName));
            result.AddRange(GetDebuffs(luaunitName));
            return result;
        }

        public List<string> GetBuffs(string luaunitName)
        {
            List<string> resultLowered = new List<string>();
            StringBuilder cmdBuffs = new StringBuilder();
            cmdBuffs.Append("local buffs, i = { }, 1;");
            cmdBuffs.Append("local buff = UnitBuff(\"").Append(luaunitName).Append("\", i);");
            cmdBuffs.Append("while buff do\n");
            cmdBuffs.Append("buffs[#buffs + 1] = buff;");
            cmdBuffs.Append("i = i + 1;");
            cmdBuffs.Append("buff = UnitBuff(\"").Append(luaunitName).Append("\", i);");
            cmdBuffs.Append("end;");
            cmdBuffs.Append("if #buffs < 1 then\n");
            cmdBuffs.Append("buffs = \"\";");
            cmdBuffs.Append("else\n");
            cmdBuffs.Append("activeUnitBuffs = table.concat(buffs, \", \");");
            cmdBuffs.Append("end;");

            LuaDoString(cmdBuffs.ToString());
            string[] buffs = GetLocalizedText("activeUnitBuffs").Split(',');

            foreach (string s in buffs)
            {
                resultLowered.Add(s.Trim().ToLower());
            }

            return resultLowered;
        }

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

        public string GetItemStats(string itemLink)
        {
            string command = $"abotItemLink=\"{itemLink}\"abotItemStatsResult=''stats={{}}abStats=GetItemStats(abotItemLink,stats)abotItemStatsResult='{{'..'\"stamina\": \"'..tostring(stats[\"ITEM_MOD_STAMINA_SHORT\"]or 0)..'\",'..'\"agility\": \"'..tostring(stats[\"ITEM_MOD_AGILITY_SHORT\"]or 0)..'\",'..'\"strenght\": \"'..tostring(stats[\"ITEM_MOD_STRENGHT_SHORT\"]or 0)..'\",'..'\"intellect\": \"'..tostring(stats[\"ITEM_MOD_INTELLECT_SHORT\"]or 0)..'\",'..'\"spirit\": \"'..tostring(stats[\"ITEM_MOD_SPIRIT_SHORT\"]or 0)..'\",'..'\"attackpower\": \"'..tostring(stats[\"ITEM_MOD_ATTACK_POWER_SHORT\"]or 0)..'\",'..'\"spellpower\": \"'..tostring(stats[\"ITEM_MOD_SPELL_POWER_SHORT\"]or 0)..'\",'..'\"mana\": \"'..tostring(stats[\"ITEM_MOD_MANA_SHORT\"]or 0)..'\"'..'}}'";

            LuaDoString(command);
            return GetLocalizedText("abotItemStatsResult");
        }

        public string GetLootRollItemLink(int rollId)
        {
            LuaDoString($"abRollItemLink = GetLootRollItemLink({rollId});");
            return GetLocalizedText("abRollItemLink");
        }

        public string GetItemBySlot(int itemslot)
        {
            string command = $"abotItemSlot={itemslot};abotItemInfoResult='noItem';abId=GetInventoryItemID('player',abotItemSlot);abCount=GetInventoryItemCount('player',abotItemSlot);abQuality=GetInventoryItemQuality('player',abotItemSlot);abCurrentDurability,abMaxDurability=GetInventoryItemDurability(abotItemSlot);abCooldownStart,abCooldownEnd=GetInventoryItemCooldown('player',abotItemSlot);abName,abLink,abRarity,abLevel,abMinLevel,abType,abSubType,abStackCount,abEquipLoc,abIcon,abSellPrice=GetItemInfo(GetInventoryItemLink('player',abotItemSlot));abotItemInfoResult='{{'..'\"id\": \"'..tostring(abId or 0)..'\",'..'\"count\": \"'..tostring(abCount or 0)..'\",'..'\"quality\": \"'..tostring(abQuality or 0)..'\",'..'\"curDurability\": \"'..tostring(abCurrentDurability or 0)..'\",'..'\"maxDurability\": \"'..tostring(abMaxDurability or 0)..'\",'..'\"cooldownStart\": \"'..tostring(abCooldownStart or 0)..'\",'..'\"cooldownEnd\": '..tostring(abCooldownEnd or 0)..','..'\"name\": \"'..tostring(abName or 0)..'\",'..'\"link\": \"'..tostring(abLink or 0)..'\",'..'\"level\": \"'..tostring(abLevel or 0)..'\",'..'\"minLevel\": \"'..tostring(abMinLevel or 0)..'\",'..'\"type\": \"'..tostring(abType or 0)..'\",'..'\"subtype\": \"'..tostring(abSubType or 0)..'\",'..'\"maxStack\": \"'..tostring(abStackCount or 0)..'\",'..'\"equipslot\": \"'..tostring(abEquipLoc or 0)..'\",'..'\"sellprice\": \"'..tostring(abSellPrice or 0)..'\"'..'}}';";

            LuaDoString(command);
            return GetLocalizedText("abotItemInfoResult");
        }

        public string GetItemByNameOrLink(string itemName)
        {
            string command = $"abotItemName=\"{itemName}\";abotItemInfoResult='noItem';abName,abLink,abRarity,abLevel,abMinLevel,abType,abSubType,abStackCount,abEquipLoc,abIcon,abSellPrice=GetItemInfo(abotItemName);abotItemInfoResult='{{'..'\"id\": \"0\",'..'\"count\": \"1\",'..'\"quality\": \"'..tostring(abRarity or 0)..'\",'..'\"curDurability\": \"0\",'..'\"maxDurability\": \"0\",'..'\"cooldownStart\": \"0\",'..'\"cooldownEnd\": \"0\",'..'\"name\": \"'..tostring(abName or 0)..'\",'..'\"link\": \"'..tostring(abLink or 0)..'\",'..'\"level\": \"'..tostring(abLevel or 0)..'\",'..'\"minLevel\": \"'..tostring(abMinLevel or 0)..'\",'..'\"type\": \"'..tostring(abType or 0)..'\",'..'\"subtype\": \"'..tostring(abSubType or 0)..'\",'..'\"maxStack\": \"'..tostring(abStackCount or 0)..'\",'..'\"equiplocation\": \"'..tostring(abEquipLoc or 0)..'\",'..'\"sellprice\": \"'..tostring(abSellPrice or 0)..'\"'..'}}';";

            LuaDoString(command);
            return GetLocalizedText("abotItemInfoResult");
        }

        public List<string> GetDebuffs(string luaunitName)
        {
            List<string> resultLowered = new List<string>();
            StringBuilder cmdDebuffs = new StringBuilder();
            cmdDebuffs.Append("local buffs, i = { }, 1;");
            cmdDebuffs.Append("local buff = UnitDebuff(\"").Append(luaunitName).Append("\", i);");
            cmdDebuffs.Append("while buff do\n");
            cmdDebuffs.Append("buffs[#buffs + 1] = buff;");
            cmdDebuffs.Append("i = i + 1;");
            cmdDebuffs.Append("buff = UnitDebuff(\"").Append(luaunitName).Append("\", i);");
            cmdDebuffs.Append("end;");
            cmdDebuffs.Append("if #buffs < 1 then\n");
            cmdDebuffs.Append("buffs = \"\";");
            cmdDebuffs.Append("else\n");
            cmdDebuffs.Append("activeUnitDebuffs = table.concat(buffs, \", \");");
            cmdDebuffs.Append("end;");

            LuaDoString(cmdDebuffs.ToString());
            string[] debuffs = GetLocalizedText("activeUnitDebuffs").Split(',');

            foreach (string s in debuffs)
            {
                resultLowered.Add(s.Trim().ToLower());
            }

            return resultLowered;
        }

        public string GetLocalizedText(string variable)
        {
            AmeisenLogger.Instance.Log($"GetLocalizedText: {variable}...", LogLevel.Verbose);

            if (variable.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(variable);
                if (XMemory.AllocateMemory((uint)bytes.Length + 1, out IntPtr memAlloc))
                {
                    XMemory.WriteBytes(memAlloc, bytes);

                    if (memAlloc == IntPtr.Zero)
                    {
                        return string.Empty;
                    }

                    string[] asmLocalText = new string[]
                    {
                        $"CALL 0x{OffsetList.FunctionGetActivePlayerObject.ToString("X")}",
                        "MOV ECX, EAX",
                        "PUSH -1",
                        $"PUSH 0x{memAlloc.ToString("X")}",
                        $"CALL 0x{OffsetList.FunctionGetLocalizedText.ToString("X")}",
                        "RETN",
                    };

                    string result = Encoding.UTF8.GetString(InjectAndExecute(asmLocalText, true));
                    XMemory.FreeMemory(memAlloc);
                    return result;
                }
            }

            return string.Empty;
        }

        public string GetSpells()
        {
            string command = "abotSpellResult='['tabCount=GetNumSpellTabs()for a=1,tabCount do tabName,tabTexture,tabOffset,numEntries=GetSpellTabInfo(a)for b=tabOffset+1,tabOffset+numEntries do abSpellName,abSpellRank=GetSpellName(b,\"BOOKTYPE_SPELL\")if abSpellName then abName,abRank,_,abCosts,_,_,abCastTime,abMinRange,abMaxRange=GetSpellInfo(abSpellName,abSpellRank)abotSpellResult=abotSpellResult..'{'..'\"spellbookName\": \"'..tostring(tabName or 0)..'\",'..'\"spellbookId\": \"'..tostring(a or 0)..'\",'..'\"name\": \"'..tostring(abSpellName or 0)..'\",'..'\"rank\": \"'..tostring(abRank or 0)..'\",'..'\"castTime\": \"'..tostring(abCastTime or 0)..'\",'..'\"minRange\": \"'..tostring(abMinRange or 0)..'\",'..'\"maxRange\": \"'..tostring(abMaxRange or 0)..'\",'..'\"costs\": \"'..tostring(abCosts or 0)..'\"'..'}'if a<tabCount or b<tabOffset+numEntries then abotSpellResult=abotSpellResult..','end end end end;abotSpellResult=abotSpellResult..']'";
            LuaDoString(command);
            return GetLocalizedText("abotSpellResult");
        }

        public double GetSpellCooldown(string spellName)
        {
            LuaDoString($"start,duration,enabled = GetSpellCooldown(\"{spellName}\");cdLeft = (start + duration - GetTime());");
            string result = GetLocalizedText("cdLeft").Replace(".", ",");

            if (double.TryParse(result, out double value))
            {
                value = Math.Round(value, 0);
                return value > 0 ? value : 0;
            }

            return -1;
        }

        /// <summary>
        /// Check if the WowLuaUnit is casting or channeling a spell
        /// </summary>
        /// <param name="luaunit">player, target, party1...</param>
        /// <returns>(Spellname, duration)</returns>
        public (string, int) GetUnitCastingInfo(WowLuaUnit luaunit)
        {
            string cmd = $"abCastingInfo = \"none,0\"; abSpellName, x, x, x, x, abSpellEndTime = UnitCastingInfo(\"{luaunit.ToString()}\"); abDuration = ((abSpellEndTime/1000) - GetTime()) * 1000; abCastingInfo = abSpellName..\",\"..abDuration;";
            LuaDoString(cmd);
            string str = GetLocalizedText("abCastingInfo");

            if (double.TryParse(str.Split(',')[1], out double timeRemaining))
            {
                return (str.Split(',')[0], (int)Math.Round(timeRemaining, 0));
            }

            return (string.Empty, -1);
        }

        public WowUnitReaction GetUnitReaction(WowUnit wowUnitA, WowUnit wowUnitB)
        {
            WowUnitReaction reaction = WowUnitReaction.Unknown;

            if (wowUnitA == null || wowUnitB == null)
            {
                return reaction;
            }

            if (BotCache.TryGetReaction(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate, out WowUnitReaction cachedReaction))
            {
                return cachedReaction;
            }

            // integer to save the reaction
            XMemory.AllocateMemory(4, out IntPtr memAlloc);
            XMemory.Write(memAlloc, 0);

            string[] asm = new string[]
            {
                $"PUSH {wowUnitA.BaseAddress}",
                $"MOV ECX, {wowUnitB.BaseAddress}",
                $"CALL {OffsetList.FunctionGetUnitReaction}",
                $"MOV [{memAlloc}], EAX",
                "RETN",
            };

            // we need this, to be very accurate, otherwise wow will crash
            if (XMemory.ReadStruct(IntPtr.Add(wowUnitA.DescriptorAddress, OffsetList.DescriptorUnitFlags.ToInt32()), out BitVector32 unitFlagsA))
            {
                wowUnitA.UnitFlags = unitFlagsA;
            }
            else
            {
                return reaction;
            }

            if (XMemory.ReadStruct(IntPtr.Add(wowUnitB.DescriptorAddress, OffsetList.DescriptorUnitFlags.ToInt32()), out BitVector32 unitFlagsB))
            {
                wowUnitB.UnitFlags = unitFlagsB;
            }
            else
            {
                return reaction;
            }

            if (wowUnitA.IsDead || wowUnitB.IsDead)
            {
                return reaction;
            }

            try
            {
                InjectAndExecute(asm, true);
                XMemory.Read(memAlloc, out reaction);

                BotCache.CacheReaction(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate, reaction);
            }
            finally
            {
                XMemory.FreeMemory(memAlloc);
            }

            return reaction;
        }

        public byte[] InjectAndExecute(string[] asm, bool readReturnBytes)
        {
            AmeisenLogger.Instance.Log($"Injecting: {JsonConvert.SerializeObject(asm)}...", LogLevel.Verbose);

            List<byte> returnBytes = new List<byte>();

            if (!ObjectManager.IsWorldLoaded)
            {
                return returnBytes.ToArray();
            }

            try
            {
                int timeoutCounter = 0;

                // wait for the code to be executed
                while (IsInjectionUsed)
                {
                    if (timeoutCounter == 500)
                    {
                        return Array.Empty<byte>();
                    }

                    timeoutCounter++;
                    Thread.Sleep(1);
                }

                IsInjectionUsed = true;

                // preparing to inject the given ASM
                XMemory.Fasm.Clear();

                // add all lines
                foreach (string s in asm)
                {
                    XMemory.Fasm.AddLine(s);
                }

                // inject it
                XMemory.SuspendMainThread();
                XMemory.Fasm.Inject((uint)CodecaveForExecution.ToInt32());
                XMemory.ResumeMainThread();

                // now there is code to be executed
                XMemory.Write(CodeToExecuteAddress, 1);

                timeoutCounter = 0;

                // wait for the code to be executed
                while (XMemory.Read(CodeToExecuteAddress, out int codeToBeExecuted) && codeToBeExecuted > 0)
                {
                    if (timeoutCounter == 500)
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
                        XMemory.Read(ReturnValueAddress, out IntPtr dwAddress);

                        // read all parameter-bytes until we the buffer is 0
                        XMemory.ReadByte(dwAddress, out byte buffer);
                        while (buffer != 0)
                        {
                            returnBytes.Add(buffer);
                            dwAddress = IntPtr.Add(dwAddress, 1);
                            XMemory.ReadByte(dwAddress, out buffer);
                        }
                    }
                    catch (Exception e)
                    {
                        AmeisenLogger.Instance.Log($"Failed to read return bytes:\n{e.ToString()}", LogLevel.Error);
                    }
                }

                IsInjectionUsed = false;
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log($"Failed to inject:\n{e.ToString()}", LogLevel.Error);
                // now there is no more code to be executed
                XMemory.Write(CodeToExecuteAddress, 0);
                IsInjectionUsed = false;
                XMemory.ResumeMainThread();
            }

            return returnBytes.ToArray();
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

        public void KickNpcsOutOfMammoth()
            => LuaDoString("for i = 1, 2 do EjectPassengerFromSeat(i) end");

        public void LootEveryThing()
            => LuaDoString("abLootCount=GetNumLootItems();for i = abLootCount,1,-1 do LootSlot(i); ConfirmLootSlot(i); end");

        public void LuaDoString(string command)
        {
            AmeisenLogger.Instance.Log($"LuaDoString: {command}...", LogLevel.Verbose);

            if (command.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(command);
                if (XMemory.AllocateMemory((uint)bytes.Length + 1, out IntPtr memAlloc))
                {
                    XMemory.WriteBytes(memAlloc, bytes);

                    if (memAlloc == IntPtr.Zero)
                    {
                        return;
                    }

                    string[] asm = new string[]
                    {
                    $"MOV EAX, 0x{memAlloc.ToString("X")}",
                    "PUSH 0",
                    "PUSH EAX",
                    "PUSH EAX",
                    $"CALL 0x{OffsetList.FunctionLuaDoString.ToString("X")}",
                    "ADD ESP, 0xC",
                    "RETN",
                    };

                    InjectAndExecute(asm, false);
                    XMemory.FreeMemory(memAlloc);
                }
            }
        }

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

        public void ReleaseSpirit()
            => LuaDoString("RepopMe();");

        public void RepairAllItems()
            => LuaDoString("RepairAllItems();");

        public void RetrieveCorpse()
            => LuaDoString("RetrieveCorpse();");

        public void RightClickUnit(WowUnit wowUnit)
        {
            string[] asm = new string[]
            {
                $"MOV ECX, 0x{wowUnit.BaseAddress.ToString("X")}",
                "MOV EAX, DWORD[ECX]",
                "MOV EAX, DWORD[EAX + 88H]",
                "CALL EAX",
                "RETN",
            };
            InjectAndExecute(asm, false);
        }

        public void SellAllGrayItems()
            => LuaDoString("local p,N,n=0 for b=0,4 do for s=1,GetContainerNumSlots(b) do n=GetContainerItemLink(b,s) if n and string.find(n,\"9d9d9d\") then N={GetItemInfo(n)} p=p+N[11] UseContainerItem(b,s) print(\"Sold: \"..n) end end end print(\"Total: \"..GetCoinText(p))");

        public void SendChatMessage(string message)
            => LuaDoString($"DEFAULT_CHAT_FRAME.editBox:SetText(\"{message}\") ChatEdit_SendText(DEFAULT_CHAT_FRAME.editBox, 0)");

        public bool SetupEndsceneHook()
        {
            AmeisenLogger.Instance.Log("Setting up the EndsceneHook...", LogLevel.Verbose);
            EndsceneAddress = GetEndScene();

            // first thing thats 5 bytes big is here
            // we are going to replace this 5 bytes with
            // our JMP instruction (JMP (1 byte) + Address (4 byte))
            EndsceneAddress = IntPtr.Add(EndsceneAddress, ENDSCENE_HOOK_OFFSET);
            EndsceneReturnAddress = IntPtr.Add(EndsceneAddress, 0x5);

            AmeisenLogger.Instance.Log($"Endscene is at: {EndsceneAddress.ToString("X")}", LogLevel.Verbose);

            // if WoW is already hooked, unhook it
            if (IsWoWHooked)
            {
                DisposeHook();
            }
            else
            {
                if (XMemory.ReadBytes(EndsceneAddress, 5, out byte[] bytes))
                {
                    OriginalEndsceneBytes = bytes;
                }

                if (!AllocateCodeCaves())
                {
                    return false;
                }

                XMemory.Fasm.Clear();

                // save registers
                XMemory.Fasm.AddLine("PUSHFD");
                XMemory.Fasm.AddLine("PUSHAD");

                // check for code to be executed
                XMemory.Fasm.AddLine($"MOV EBX, [{CodeToExecuteAddress.ToInt32()}]");
                XMemory.Fasm.AddLine("TEST EBX, 1");
                XMemory.Fasm.AddLine("JE @out");

                // set register back to zero
                XMemory.Fasm.AddLine("XOR EBX, EBX");

                // check for world to be loaded
                // we dont want to execute code in
                // the loadingscreen, cause that
                // mostly results in crashes
                XMemory.Fasm.AddLine($"MOV EBX, [{OffsetList.IsWorldLoaded.ToInt32()}]");
                XMemory.Fasm.AddLine("TEST EBX, 1");
                XMemory.Fasm.AddLine("JE @out");

                // execute our stuff and get return address
                XMemory.Fasm.AddLine($"MOV EDX, {CodecaveForExecution.ToInt32()}");
                XMemory.Fasm.AddLine("CALL EDX");
                XMemory.Fasm.AddLine($"MOV [{ReturnValueAddress.ToInt32()}], EAX");

                // finish up our execution
                XMemory.Fasm.AddLine("@out:");
                XMemory.Fasm.AddLine("MOV EDX, 0");
                XMemory.Fasm.AddLine($"MOV [{CodeToExecuteAddress.ToInt32()}], EDX");

                // restore registers
                XMemory.Fasm.AddLine("POPAD");
                XMemory.Fasm.AddLine("POPFD");

                byte[] asmBytes = XMemory.Fasm.Assemble();

                // needed to determine the position where the original
                // asm is going to be placed
                int asmLenght = asmBytes.Length;

                // inject the instructions into our codecave
                XMemory.Fasm.Inject((uint)CodecaveForCheck.ToInt32());

                // ---------------------------------------------------
                // End of the code that checks if there is asm to be
                // executed on our hook
                // ---------------------------------------------------

                // Prepare to replace the instructions inside WoW
                XMemory.Fasm.Clear();

                // do the original EndScene stuff after we restored the registers
                // and insert it after our code
                XMemory.WriteBytes(IntPtr.Add(CodecaveForCheck, asmLenght), OriginalEndsceneBytes);
                AmeisenLogger.Instance.Log($"EndsceneHook OriginalEndsceneBytes: {JsonConvert.SerializeObject(OriginalEndsceneBytes)}", LogLevel.Verbose);

                // return to original function after we're done with our stuff
                XMemory.Fasm.AddLine($"JMP {EndsceneReturnAddress.ToInt32()}");
                XMemory.Fasm.Inject((uint)CodecaveForCheck.ToInt32() + (uint)asmLenght + 5);
                XMemory.Fasm.Clear();

                // ---------------------------------------------------
                // End of doing the original stuff and returning to
                // the original instruction
                // ---------------------------------------------------

                // modify original EndScene instructions to start the hook
                XMemory.Fasm.AddLine($"JMP {CodecaveForCheck.ToInt32()}");
                XMemory.Fasm.Inject((uint)EndsceneAddress.ToInt32());

                AmeisenLogger.Instance.Log("EndsceneHook Successful...", LogLevel.Verbose);

                // we should've hooked WoW now
                return true;
            }

            return false;
        }

        public void StartAutoAttack() => SendChatMessage("/startattack");

        public void TargetGuid(ulong guid)
        {
            byte[] guidBytes = BitConverter.GetBytes(guid);
            string[] asm = new string[]
            {
                $"PUSH {BitConverter.ToUInt32(guidBytes, 4)}",
                $"PUSH {BitConverter.ToUInt32(guidBytes, 0)}",
                $"CALL 0x{OffsetList.FunctionSetTarget.ToString("X")}",
                "ADD ESP, 0x8",
                "RETN"
            };
            InjectAndExecute(asm, false);
        }

        public void TargetLuaUnit(WowLuaUnit unit)
            => LuaDoString($"TargetUnit(\"{unit.ToString()}\");");

        public void TargetNearestEnemy()
            => SendChatMessage("/targetenemy [harm][nodead]");

        public void ClearTarget()
            => SendChatMessage("/cleartarget");

        public void ClearTargetIfDead()
            => SendChatMessage("/cleartarget [dead]");

        public void WriteCtmValues(Vector3 targetPosition, ClickToMoveType clickToMoveType = ClickToMoveType.Move, float distance = 1.5f)
        {
            XMemory.Write(OffsetList.ClickToMoveX, targetPosition.X);
            XMemory.Write(OffsetList.ClickToMoveY, targetPosition.Y);
            XMemory.Write(OffsetList.ClickToMoveZ, targetPosition.Z);
            XMemory.Write(OffsetList.ClickToMoveDistance, distance);
            XMemory.Write(OffsetList.ClickToMoveAction, clickToMoveType);
        }

        private bool AllocateCodeCaves()
        {
            AmeisenLogger.Instance.Log("Allocating Codecaves for the EndsceneHook...", LogLevel.Verbose);

            // integer to check if there is code waiting to be executed
            if (!XMemory.AllocateMemory(4, out IntPtr codeToExecuteAddress))
            {
                return false;
            }

            CodeToExecuteAddress = codeToExecuteAddress;
            XMemory.Write(CodeToExecuteAddress, 0);
            AmeisenLogger.Instance.Log($"EndsceneHook CodeToExecuteAddress: {codeToExecuteAddress.ToString("X")}", LogLevel.Verbose);

            // integer to save the pointer to the return value
            if (!XMemory.AllocateMemory(4, out IntPtr returnValueAddress))
            {
                return false;
            }

            ReturnValueAddress = returnValueAddress;
            XMemory.Write(ReturnValueAddress, 0);
            AmeisenLogger.Instance.Log($"EndsceneHook ReturnValueAddress: {returnValueAddress.ToString("X")}", LogLevel.Verbose);

            // codecave to check wether we need to execute something
            if (!XMemory.AllocateMemory(128, out IntPtr codecaveForCheck))
            {
                return false;
            }

            CodecaveForCheck = codecaveForCheck;
            AmeisenLogger.Instance.Log($"EndsceneHook CodecaveForCheck: {codecaveForCheck.ToString("X")}", LogLevel.Verbose);

            // codecave for the code we wan't to execute
            if (!XMemory.AllocateMemory(2048, out IntPtr codecaveForExecution))
            {
                return false;
            }

            CodecaveForExecution = codecaveForExecution;
            AmeisenLogger.Instance.Log($"EndsceneHook CodecaveForExecution: {codecaveForExecution.ToString("X")}", LogLevel.Verbose);

            return true;
        }

        private IntPtr GetEndScene()
        {
            if (XMemory.Read(OffsetList.EndSceneStaticDevice, out IntPtr pDevice)
                && XMemory.Read(IntPtr.Add(pDevice, OffsetList.EndSceneOffsetDevice.ToInt32()), out IntPtr pEnd)
                && XMemory.Read(pEnd, out IntPtr pScene)
                && XMemory.Read(IntPtr.Add(pScene, OffsetList.EndSceneOffset.ToInt32()), out IntPtr pEndscene))
            {
                return pEndscene;
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        public string GetInventoryItems()
        {
            string command = "abotInventoryResult='['for a=0,4 do containerSlots=GetContainerNumSlots(a)for b=1,containerSlots do abItemLink=GetContainerItemLink(a,b)if abItemLink then abCurrentDurability,abMaxDurability=GetContainerItemDurability(a,b)abCooldownStart,abCooldownEnd=GetContainerItemCooldown(a,b)abIcon,abItemCount,abLocked,abQuality,abReadable,abLootable,abItemLink,isFiltered=GetContainerItemInfo(a,b)abName,abLink,abRarity,abLevel,abMinLevel,abType,abSubType,abStackCount,abEquipLoc,abIcon,abSellPrice=GetItemInfo(abItemLink)abotInventoryResult=abotInventoryResult..'{'..'\"id\": \"'..tostring(abId or 0)..'\",'..'\"count\": \"'..tostring(abItemCount or 0)..'\",'..'\"quality\": \"'..tostring(abQuality or 0)..'\",'..'\"curDurability\": \"'..tostring(abCurrentDurability or 0)..'\",'..'\"maxDurability\": \"'..tostring(abMaxDurability or 0)..'\",'..'\"cooldownStart\": \"'..tostring(abCooldownStart or 0)..'\",'..'\"cooldownEnd\": \"'..tostring(abCooldownEnd or 0)..'\",'..'\"name\": \"'..tostring(abName or 0)..'\",'..'\"lootable\": \"'..tostring(abLootable or 0)..'\",'..'\"readable\": \"'..tostring(abReadable or 0)..'\",'..'\"link\": \"'..tostring(abItemLink or 0)..'\",'..'\"level\": \"'..tostring(abLevel or 0)..'\",'..'\"minLevel\": \"'..tostring(abMinLevel or 0)..'\",'..'\"type\": \"'..tostring(abType or 0)..'\",'..'\"subtype\": \"'..tostring(abSubType or 0)..'\",'..'\"maxStack\": \"'..tostring(abStackCount or 0)..'\",'..'\"equiplocation\": \"'..tostring(abEquipLoc or 0)..'\",'..'\"sellprice\": \"'..tostring(abSellPrice or 0)..'\"'..'\"bagid\": \"'..tostring(a or 0)..'\"'..'\"bagslot\": \"'..tostring(b or 0)..'\"'..'}'if b<containerSlots then abotInventoryResult=abotInventoryResult..','end end end end;abotInventoryResult=abotInventoryResult..']'";
            LuaDoString(command);

            return GetLocalizedText("abotInventoryResult");
        }

        public string GetEquipmentItems()
        {
            string command = "abotItemInfoResult='['for a=0,23 do abId=GetInventoryItemID('player',a)abCount=GetInventoryItemCount('player',a)abCurrentDurability,abMaxDurability=GetInventoryItemDurability(a)abCooldownStart,abCooldownEnd=GetInventoryItemCooldown('player',a)abName,abLink,abRarity,abLevel,abMinLevel,abType,abSubType,abStackCount,abEquipLoc,abIcon,abSellPrice=GetItemInfo(GetInventoryItemLink('player',a))abotItemInfoResult=abotItemInfoResult..'{'..'\"id\": \"'..tostring(abId or 0)..'\",'..'\"count\": \"'..tostring(abCount or 0)..'\",'..'\"quality\": \"'..tostring(abRarity or 0)..'\",'..'\"curDurability\": \"'..tostring(abCurrentDurability or 0)..'\",'..'\"maxDurability\": \"'..tostring(abMaxDurability or 0)..'\",'..'\"cooldownStart\": \"'..tostring(abCooldownStart or 0)..'\",'..'\"cooldownEnd\": '..tostring(abCooldownEnd or 0)..','..'\"name\": \"'..tostring(abName or 0)..'\",'..'\"link\": \"'..tostring(abLink or 0)..'\",'..'\"level\": \"'..tostring(abLevel or 0)..'\",'..'\"minLevel\": \"'..tostring(abMinLevel or 0)..'\",'..'\"type\": \"'..tostring(abType or 0)..'\",'..'\"subtype\": \"'..tostring(abSubType or 0)..'\",'..'\"maxStack\": \"'..tostring(abStackCount or 0)..'\",'..'\"equiplocation\": \"'..tostring(a or 0)..'\",'..'\"sellprice\": \"'..tostring(abSellPrice or 0)..'\"'..'}'if abotItemInfoResult<23 then abotItemInfoResult=abotItemInfoResult..\",\"end end;abotItemInfoResult=abotItemInfoResult..']'";
            LuaDoString(command);
            return GetLocalizedText("abotItemInfoResult");
        }
    }
}