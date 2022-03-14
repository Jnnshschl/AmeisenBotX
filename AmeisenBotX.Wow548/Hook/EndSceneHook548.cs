using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Hook;
using AmeisenBotX.Wow548.Offsets;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace AmeisenBotX.Wow548.Hook
{
    public class EndSceneHook548 : GenericEndSceneHook
    {
        public EndSceneHook548(WowMemoryApi memory)
            : base(memory)
        {
            OriginalFunctionBytes = new();
        }

        /// <summary>
        /// Used to save the old render flags of wow.
        /// </summary>
        private int OldRenderFlags { get; set; }

        /// <summary>
        /// Used to save the original instruction when a function get disabled.
        /// </summary>
        private Dictionary<IntPtr, byte> OriginalFunctionBytes { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CallObjectFunction(IntPtr objectBaseAddress, IntPtr functionAddress, List<object>? args = null)
        {
            return CallObjectFunction(objectBaseAddress, functionAddress, args, false, out _);
        }

        /// <summary>
        /// Use this to call a thiscall function of a wowobject
        /// </summary>
        /// <param name="objectBaseAddress">Object base</param>
        /// <param name="functionAddress">Function to call</param>
        /// <param name="args">Arguments, can be null</param>
        /// <param name="readReturnBytes">Whether to read the retunr address or not</param>
        /// <param name="returnAddress">Return address</param>
        /// <returns>True if everything went right, false if not</returns>
        public bool CallObjectFunction(IntPtr objectBaseAddress, IntPtr functionAddress, List<object>? args, bool readReturnBytes, out IntPtr returnAddress)
        {
#if DEBUG
            if (objectBaseAddress == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(objectBaseAddress), "objectBaseAddress is an invalid pointer"); }
            if (functionAddress == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(functionAddress), "functionAddress is an invalid pointer"); }
#endif
            List<string> asm = new() { $"MOV ECX, {objectBaseAddress}" };

            if (args != null)
            {
                for (int i = 0; i < args.Count; ++i)
                {
                    asm.Add($"PUSH {args[i]}");
                }
            }

            asm.Add($"CALL {functionAddress}");
            asm.Add("RET");

            if (readReturnBytes)
            {
                bool status = InjectAndExecute(asm, readReturnBytes, out returnAddress);
                return status;
            }

            returnAddress = IntPtr.Zero;
            return InjectAndExecute(asm, readReturnBytes, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaAbandonQuestsNotIn(IEnumerable<string> questNames)
        {
            if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetNumQuestLogEntries()"), out string r1)
                && int.TryParse(r1, out int numQuestLogEntries))
            {
                for (int i = 1; i <= numQuestLogEntries; i++)
                {
                    if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetQuestLogTitle({i})"), out string questLogTitle) && !questNames.Contains(questLogTitle))
                    {
                        LuaDoString($"SelectQuestLogEntry({i});SetAbandonQuest();AbandonQuest()");
                        break;
                    }
                }
            }
        }

        public bool LuaDoString(string command)
        {
#if DEBUG
            if (string.IsNullOrWhiteSpace(command)) { throw new ArgumentOutOfRangeException(nameof(command), "command is empty"); }
#endif
            AmeisenLogger.I.Log("548Hook", $"LuaDoString: {command}", LogLevel.Verbose);

            byte[] bytes = Encoding.UTF8.GetBytes(command + "\0");

            if (Memory.AllocateMemory((uint)bytes.Length, out IntPtr memAlloc))
            {
                try
                {
                    if (Memory.WriteBytes(memAlloc, bytes))
                    {
                        return InjectAndExecute(new string[]
                        {
                            "PUSH 0",
                            $"PUSH {memAlloc}",
                            $"PUSH {memAlloc}",
                            $"CALL {Memory.Offsets.FunctionLuaDoString}",
                            "ADD ESP, 0xC",
                            "RET",
                        });
                    }
                }
                finally
                {
                    Memory.FreeMemory(memAlloc);
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ObjectRightClick(IntPtr objectBase)
        {
            CallObjectFunction(objectBase, Memory.Offsets.FunctionGameobjectOnRightClick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFacing(IntPtr unitBase, float angle, bool smooth = false)
        {
            CallObjectFunction(unitBase, smooth ? ((OffsetList548)Memory.Offsets).FunctionUnitSetFacingSmooth : Memory.Offsets.FunctionUnitSetFacing, new()
            {
                angle.ToString(CultureInfo.InvariantCulture).Replace(',', '.'),
                Environment.TickCount
            });
        }

        public void SetRenderState(bool renderingEnabled)
        {
            if (renderingEnabled)
            {
                LuaDoString("WorldFrame:Show();UIParent:Show()");
            }

            Memory.SuspendMainThread();

            if (renderingEnabled)
            {
                EnableFunction(Memory.Offsets.FunctionWorldRender);
                EnableFunction(Memory.Offsets.FunctionWorldRenderWorld);
                EnableFunction(Memory.Offsets.FunctionWorldFrame);

                Memory.Write(Memory.Offsets.RenderFlags, OldRenderFlags);
            }
            else
            {
                if (Memory.Read(Memory.Offsets.RenderFlags, out int renderFlags))
                {
                    OldRenderFlags = renderFlags;
                }

                DisableFunction(Memory.Offsets.FunctionWorldRender);
                DisableFunction(Memory.Offsets.FunctionWorldRenderWorld);
                DisableFunction(Memory.Offsets.FunctionWorldFrame);

                Memory.Write(Memory.Offsets.RenderFlags, 0);
            }

            Memory.ResumeMainThread();

            if (!renderingEnabled)
            {
                LuaDoString("WorldFrame:Hide();UIParent:Hide()");
            }
        }

        public void TargetGuid(ulong guid)
        {
            byte[] guidBytes = BitConverter.GetBytes(guid);

            InjectAndExecute(new string[]
            {
                $"PUSH {BitConverter.ToUInt32(guidBytes, 4)}",
                $"PUSH {BitConverter.ToUInt32(guidBytes, 0)}",
                $"CALL {Memory.Offsets.FunctionSetTarget}",
                "ADD ESP, 0x8",
                "RET"
            });
        }

        public bool TraceLine(Vector3 start, Vector3 end, uint flags)
        {
            if (Memory.AllocateMemory(40, out IntPtr tracelineCodecave))
            {
                try
                {
                    (float, Vector3, Vector3) tracelineCombo = (1.0f, start, end);

                    IntPtr distancePointer = tracelineCodecave;
                    IntPtr startPointer = IntPtr.Add(distancePointer, 0x4);
                    IntPtr endPointer = IntPtr.Add(startPointer, 0xC);
                    IntPtr resultPointer = IntPtr.Add(endPointer, 0xC);

                    if (Memory.Write(distancePointer, tracelineCombo))
                    {
                        string[] asm = new string[]
                        {
                            "PUSH 0",
                            $"PUSH {flags}",
                            $"PUSH {distancePointer}",
                            $"PUSH {resultPointer}",
                            $"PUSH {endPointer}",
                            $"PUSH {startPointer}",
                            $"CALL {Memory.Offsets.FunctionTraceline}",
                            "ADD ESP, 0x18",
                            "RET",
                        };

                        if (InjectAndExecute(asm, true, out IntPtr returnAddress))
                        {
                            return returnAddress != IntPtr.Zero && (returnAddress.ToInt32() & 0xFF) == 0;
                        }
                    }
                }
                finally
                {
                    Memory.FreeMemory(tracelineCodecave);
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InteractWithUnit(ulong guid)
        {
            byte[] guidBytes = BitConverter.GetBytes(guid);

            InjectAndExecute(new string[]
            {
                $"PUSH {BitConverter.ToUInt32(guidBytes, 4)}",
                $"PUSH {BitConverter.ToUInt32(guidBytes, 0)}",
                $"CALL {Memory.Offsets.FunctionUnitOnRightClick}",
                "ADD ESP, 0x8",
                "RET"
            });
        }

        public void ClickOnTerrain(Vector3 position)
        {
            if (Memory.AllocateMemory(20, out IntPtr codeCaveVector3))
            {
                try
                {
                    if (Memory.Write(IntPtr.Add(codeCaveVector3, 8), position))
                    {
                        InjectAndExecute(new string[]
                        {
                            $"PUSH {codeCaveVector3.ToInt32()}",
                            $"CALL {Memory.Offsets.FunctionHandleTerrainClick}",
                            "ADD ESP, 0x4",
                            "RET",
                        });
                    }
                }
                finally
                {
                    Memory.FreeMemory(codeCaveVector3);
                }
            }
        }

        public void ClickToMove(IntPtr playerBase, Vector3 position)
        {
            if (Memory.AllocateMemory(12, out IntPtr codeCaveVector3))
            {
                try
                {
                    if (Memory.Write(codeCaveVector3, position))
                    {
                        CallObjectFunction(playerBase, Memory.Offsets.FunctionPlayerClickToMove, new() { codeCaveVector3 });
                    }
                }
                finally
                {
                    Memory.FreeMemory(codeCaveVector3);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExecuteLuaAndRead((string, string) cmdVarTuple, out string result)
        {
            return ExecuteLuaAndRead(cmdVarTuple.Item1, cmdVarTuple.Item2, out result);
        }

        public bool ExecuteLuaAndRead(string command, string variable, out string result)
        {
#if DEBUG
            if (string.IsNullOrWhiteSpace(command)) { throw new ArgumentOutOfRangeException(nameof(command), "command is empty"); }
            if (string.IsNullOrWhiteSpace(variable)) { throw new ArgumentOutOfRangeException(nameof(variable), "variable is empty"); }
#endif
            AmeisenLogger.I.Log("548Hook", $"WowExecuteLuaAndRead: {command}", LogLevel.Verbose);

            byte[] commandBytes = Encoding.UTF8.GetBytes(command + "\0");
            byte[] variableBytes = Encoding.UTF8.GetBytes(variable + "\0");

            if (Memory.AllocateMemory((uint)commandBytes.Length + (uint)variableBytes.Length, out IntPtr memAllocCmdVar))
            {
                try
                {
                    byte[] bytesToWrite = new byte[commandBytes.Length + variableBytes.Length];

                    Array.Copy(commandBytes, bytesToWrite, commandBytes.Length);
                    Array.Copy(variableBytes, 0, bytesToWrite, commandBytes.Length, variableBytes.Length);

                    Memory.WriteBytes(memAllocCmdVar, bytesToWrite);

                    string[] asm = new string[]
                    {
                        "PUSH 0",
                        $"PUSH {memAllocCmdVar}",
                        $"PUSH {memAllocCmdVar}",
                        $"CALL {Memory.Offsets.FunctionLuaDoString}",
                        "ADD ESP, 0xC",
                        $"CALL {Memory.Offsets.FunctionGetActivePlayerObject}",
                        "MOV ECX, EAX",
                        "PUSH -1",
                        $"PUSH {memAllocCmdVar + commandBytes.Length}",
                        $"CALL {Memory.Offsets.FunctionGetLocalizedText}",
                        "RET",
                    };

                    if (InjectAndExecute(asm, true, out IntPtr returnAddress)
                        && Memory.ReadString(returnAddress, Encoding.UTF8, out result))
                    {
                        return !string.IsNullOrWhiteSpace(result);
                    }
                }
                finally
                {
                    Memory.FreeMemory(memAllocCmdVar);
                }
            }

            result = string.Empty;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FacePosition(IntPtr playerBase, Vector3 playerPosition, Vector3 positionToFace, bool smooth = false)
        {
            SetFacing(playerBase, BotMath.GetFacingAngle(playerPosition, positionToFace), smooth);
        }

        public bool GetLocalizedText(string variable, out string result)
        {
#if DEBUG
            if (string.IsNullOrWhiteSpace(variable)) { throw new ArgumentOutOfRangeException(nameof(variable), "variable is empty"); }
#endif
            if (!string.IsNullOrWhiteSpace(variable))
            {
                byte[] variableBytes = Encoding.UTF8.GetBytes(variable + "\0");

                if (Memory.AllocateMemory((uint)variableBytes.Length, out IntPtr memAlloc))
                {
                    try
                    {
                        Memory.WriteBytes(memAlloc, variableBytes);

                        string[] asm = new string[]
                        {
                            $"CALL {Memory.Offsets.FunctionGetActivePlayerObject}",
                            "MOV ECX, EAX",
                            "PUSH -1",
                            $"PUSH {memAlloc}",
                            $"CALL {Memory.Offsets.FunctionGetLocalizedText}",
                            "RET",
                        };

                        if (InjectAndExecute(asm, true, out IntPtr returnAddress)
                            && Memory.ReadString(returnAddress, Encoding.UTF8, out result))
                        {
                            return !string.IsNullOrWhiteSpace(result);
                        }
                    }
                    finally
                    {
                        Memory.FreeMemory(memAlloc);
                    }
                }
            }

            result = string.Empty;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetUnitReaction(IntPtr a, IntPtr b)
        {
#if DEBUG
            if (a == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(a), "a is no valid pointer"); }
            if (b == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(b), "b is no valid pointer"); }
#endif
            return CallObjectFunction(a, Memory.Offsets.FunctionUnitGetReaction, new() { b }, true, out IntPtr ret)
                && ret != IntPtr.Zero ? ret.ToInt32() : 2;
        }

        private void DisableFunction(IntPtr address)
        {
            // check whether we already replaced the function or not
            if (Memory.Read(address, out byte opcode)
                && opcode != 0xC3)
            {
                SaveOriginalFunctionBytes(address);
                Memory.PatchMemory(address, (byte)0xC3);
            }
        }

        private void EnableFunction(IntPtr address)
        {
            // check for RET opcode to be present before restoring original function
            if (OriginalFunctionBytes.ContainsKey(address)
                && Memory.Read(address, out byte opcode)
                && opcode == 0xC3)
            {
                Memory.PatchMemory(address, OriginalFunctionBytes[address]);
            }
        }

        private void SaveOriginalFunctionBytes(IntPtr address)
        {
            if (Memory.Read(address, out byte opcode))
            {
                if (!OriginalFunctionBytes.ContainsKey(address))
                {
                    OriginalFunctionBytes.Add(address, opcode);
                }
                else
                {
                    OriginalFunctionBytes[address] = opcode;
                }
            }
        }
    }
}