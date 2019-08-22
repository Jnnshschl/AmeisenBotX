using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Memory;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;

namespace AmeisenBotX.Core.Hook
{
    public class HookManager
    {
        public byte[] originalEndsceneBytes;
        private const int ENDSCENE_HOOK_OFFSET = 0x2;

        public IntPtr CodecaveForCheck { get; private set; }
        public IntPtr CodecaveForExecution { get; private set; }
        public IntPtr CodeToExecuteAddress { get; private set; }
        public IntPtr EndsceneAddress { get; private set; }
        public IntPtr EndsceneReturnAddress { get; private set; }
        public IntPtr ReturnValueAddress { get; private set; }

        public bool IsInjectionUsed { get; private set; }

        public bool IsWoWHooked
        {
            get
            {
                if (XMemory.ReadBytes(EndsceneAddress, 1, out byte[] c))
                    return c[0] == 0xE9;
                else
                    return false;
            }
        }

        private CacheManager CacheManager { get; }
        private ObjectManager ObjectManager { get; }
        private IOffsetList OffsetList { get; }
        private XMemory XMemory { get; }

        public HookManager(XMemory xMemory, IOffsetList offsetList, ObjectManager objectManager, CacheManager cacheManager)
        {
            XMemory = xMemory;
            OffsetList = offsetList;
            ObjectManager = objectManager;
            CacheManager = cacheManager;
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
            if (castOnSelf)
            {
                LuaDoString($"CastSpellByName(\"{name}\", true);");
            }
            else
            {
                LuaDoString($"CastSpellByName(\"{name}\");");
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

        public void FaceUnit(WowPlayer player, WowPosition positionToFace)
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
            if (variable.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(variable);
                if (XMemory.AllocateMemory((uint)bytes.Length + 1, out IntPtr memAlloc))
                {
                    XMemory.WriteBytes(memAlloc, bytes);

                    if (memAlloc == IntPtr.Zero)
                        return "";

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
            return "";
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

        public WowUnitReaction GetUnitReaction(WowUnit wowUnitA, WowUnit wowUnitB)
        {
            WowUnitReaction reaction = WowUnitReaction.Unknown;

            if (CacheManager.ReactionCache.ContainsKey((wowUnitA.FactionTemplate, wowUnitB.FactionTemplate)))
                return CacheManager.ReactionCache[(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate)];

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
                wowUnitA.UnitFlags = unitFlagsA;
            else
                return reaction;

            if (XMemory.ReadStruct(IntPtr.Add(wowUnitB.DescriptorAddress, OffsetList.DescriptorUnitFlags.ToInt32()), out BitVector32 unitFlagsB))
                wowUnitB.UnitFlags = unitFlagsB;
            else
                return reaction;

            if (wowUnitA.IsDead || wowUnitB.IsDead) return reaction;

            try
            {
                InjectAndExecute(asm, true);
                XMemory.Read(memAlloc, out reaction);

                CacheManager.ReactionCache.Add((wowUnitA.FactionTemplate, wowUnitB.FactionTemplate), reaction);
            }
            finally
            {
                XMemory.FreeMemory(memAlloc);
            }

            return reaction;
        }

        public byte[] InjectAndExecute(string[] asm, bool readReturnBytes)
        {
            List<byte> returnBytes = new List<byte>();

            if (!ObjectManager.IsWorldLoaded)
                return returnBytes.ToArray();

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

                // now there is code to be executed
                XMemory.Write(CodeToExecuteAddress, 1);
                // inject it
                XMemory.Fasm.Inject((uint)CodecaveForExecution.ToInt32());

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
                    byte buffer;
                    try
                    {
                        XMemory.Read(ReturnValueAddress, out IntPtr dwAddress);

                        // read all parameter-bytes until we the buffer is 0
                        XMemory.ReadByte(dwAddress, out buffer);
                        while (buffer != 0)
                        {
                            returnBytes.Add(buffer);
                            dwAddress = IntPtr.Add(dwAddress, 1);
                            XMemory.ReadByte(dwAddress, out buffer);
                        }
                    }
                    catch
                    {
                    }
                }
                IsInjectionUsed = false;
            }
            catch
            {
                // now there is no more code to be executed
                XMemory.Write(CodeToExecuteAddress, 0);
                IsInjectionUsed = false;
            }

            return returnBytes.ToArray();
        }

        public bool IsGhost(string unit)
        {
            LuaDoString($"isGhost = UnitIsGhost(\"{unit}\");");
            string result = GetLocalizedText("isGhost");

            if (int.TryParse(result, out int isGhost))
                return isGhost == 1;

            return false;
        }

        public void KickNpcsOutOfMammoth()
            => LuaDoString("for i = 1, 2 do EjectPassengerFromSeat(i) end");

        public void LootEveryThing()
            => LuaDoString("abLootCount=GetNumLootItems();for i = abLootCount,1,-1 do LootSlot(i); ConfirmLootSlot(i); end");

        public void LuaDoString(string command)
        {
            if (command.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(command);
                if (XMemory.AllocateMemory((uint)bytes.Length + 1, out IntPtr memAlloc))
                {
                    XMemory.WriteBytes(memAlloc, bytes);

                    if (memAlloc == IntPtr.Zero)
                        return;

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

        public void ReleaseSpirit() => LuaDoString("RepopMe();");

        public void RepairAllItems() => LuaDoString("RepairAllItems();");

        public void RetrieveCorpse() => LuaDoString("RetrieveCorpse();");

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

        public void SendChatMessage(string message)
            => LuaDoString($"DEFAULT_CHAT_FRAME.editBox:SetText(\"{message}\") ChatEdit_SendText(DEFAULT_CHAT_FRAME.editBox, 0)");

        public bool SetupEndsceneHook()
        {
            EndsceneAddress = GetEndScene();

            // first thing thats 5 bytes big is here
            // we are going to replace this 5 bytes with
            // our JMP instruction (JMP (1 byte) + Address (4 byte))
            EndsceneAddress = IntPtr.Add(EndsceneAddress, ENDSCENE_HOOK_OFFSET);
            EndsceneReturnAddress = IntPtr.Add(EndsceneAddress, 0x5);

            // if WoW is already hooked, unhook it
            if (IsWoWHooked) { DisposeHook(); }
            else
            {
                if (XMemory.ReadBytes(EndsceneAddress, 5, out byte[] bytes))
                    originalEndsceneBytes = bytes;

                if (!AllocateCodeCaves())
                    return false;

                XMemory.Fasm.Clear();
                // save registers
                XMemory.Fasm.AddLine("PUSHFD");
                XMemory.Fasm.AddLine("PUSHAD");

                // check for code to be executed
                XMemory.Fasm.AddLine($"MOV EBX, [{CodeToExecuteAddress.ToInt32()}]");
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
                XMemory.WriteBytes(IntPtr.Add(CodecaveForCheck, asmLenght), originalEndsceneBytes);

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

        public void TargetNearestEnemy() => SendChatMessage("/targetenemy [harm][nodead]");

        public void WriteCtmValues(WowPosition targetPosition, ClickToMoveType clickToMoveType = ClickToMoveType.Move, float distance = 1.5f)
        {
            XMemory.Write(OffsetList.ClickToMoveX, targetPosition.X);
            XMemory.Write(OffsetList.ClickToMoveY, targetPosition.Y);
            XMemory.Write(OffsetList.ClickToMoveZ, targetPosition.Z);
            XMemory.Write(OffsetList.ClickToMoveDistance, distance);
            XMemory.Write(OffsetList.ClickToMoveAction, clickToMoveType);
        }

        private bool AllocateCodeCaves()
        {
            // integer to check if there is code waiting to be executed
            if (!XMemory.AllocateMemory(4, out IntPtr codeToExecuteAddress))
                return false;
            CodeToExecuteAddress = codeToExecuteAddress;
            XMemory.Write(CodeToExecuteAddress, 0);

            // integer to save the address of the return value
            if (!XMemory.AllocateMemory(4, out IntPtr returnValueAddress))
                return false;
            ReturnValueAddress = returnValueAddress;
            XMemory.Write(ReturnValueAddress, 0);

            // codecave to check if we need to execute something
            if (!XMemory.AllocateMemory(128, out IntPtr codecaveForCheck))
                return false;
            CodecaveForCheck = codecaveForCheck;

            // codecave for the code we wa't to execute
            if (!XMemory.AllocateMemory(2048, out IntPtr codecaveForExecution))
                return false;
            CodecaveForExecution = codecaveForExecution;

            return true;
        }

        public void DisposeHook()
        {
            if (IsWoWHooked)
            {
                XMemory.WriteBytes(EndsceneAddress, originalEndsceneBytes);

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

        private IntPtr GetEndScene()
        {
            if (XMemory.Read(OffsetList.EndSceneStaticDevice, out IntPtr pDevice)
                && XMemory.Read(IntPtr.Add(pDevice, OffsetList.EndSceneOffsetDevice.ToInt32()), out IntPtr pEnd)
                && XMemory.Read(pEnd, out IntPtr pScene)
                && XMemory.Read(IntPtr.Add(pScene, OffsetList.EndSceneOffset.ToInt32()), out IntPtr pEndscene))
                return pEndscene;
            else
                return IntPtr.Zero;
        }
    }
}