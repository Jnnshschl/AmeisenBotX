using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateStartWow : BasicState
    {
        public StateStartWow(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
        }

        public event Action OnWoWStarted;

        public override void Enter()
        {
            AmeisenLogger.I.Log("StartWow", "Setting TOS and EULA to 1 in config");
            CheckTosAndEula();

            if (Config.AutoChangeRealmlist)
            {
                AmeisenLogger.I.Log("StartWow", "Changing Realmlist");
                ChangeRealmlist();
            }
        }

        public override void Execute()
        {
            if (File.Exists(Config.PathToWowExe))
            {
                if (Bot.Memory.Process == null || Bot.Memory.Process.HasExited)
                {
                    AmeisenLogger.I.Log("StartWow", "Starting WoW Process");
                    Process p = Bot.Memory.StartProcessNoActivate($"\"{Config.PathToWowExe}\" -windowed -d3d9", out IntPtr processHandle, out IntPtr mainThreadHandle);

                    AmeisenLogger.I.Log("StartWow", "Waiting for input idle");
                    p.WaitForInputIdle();

                    AmeisenLogger.I.Log("StartWow", $"Attaching XMemory to {p.ProcessName} ({p.Id})");

                    if (Bot.Memory.Init(p, processHandle, mainThreadHandle))
                    {
                        try
                        {
                            OnWoWStarted?.Invoke();
                        }
                        catch (Exception ex) { AmeisenLogger.I.Log("StartWow", $"Error at OnWoWStarted:\n{ex}"); }

                        AmeisenLogger.I.Log("StartWow", $"Switching to login state...");
                        StateMachine.SetState(BotState.Login);
                    }
                    else
                    {
                        AmeisenLogger.I.Log("StartWow", $"Attaching XMemory failed...");
                        p.Kill();
                    }
                }
            }
        }

        public override void Leave()
        {
            StateMachine.WowCrashed = false;
        }

        private void ChangeRealmlist()
        {
            try
            {
                string configWtfPath = Path.Combine(Directory.GetParent(Config.PathToWowExe).FullName, "wtf", "config.wtf");

                if (File.Exists(configWtfPath))
                {
                    bool editedFile = false;
                    List<string> content = File.ReadAllLines(configWtfPath).ToList();

                    if (!content.Any(e => e.Contains($"SET REALMLIST {Config.Realmlist}", StringComparison.OrdinalIgnoreCase)))
                    {
                        bool found = false;

                        for (int i = 0; i < content.Count; ++i)
                        {
                            if (content[i].Contains("SET REALMLIST", StringComparison.OrdinalIgnoreCase))
                            {
                                editedFile = true;
                                content[i] = $"SET REALMLIST {Config.Realmlist}";
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            editedFile = true;
                            content.Add($"SET REALMLIST {Config.Realmlist}");
                        }
                    }

                    if (editedFile)
                    {
                        File.SetAttributes(configWtfPath, FileAttributes.Normal);
                        File.WriteAllLines(configWtfPath, content);
                        File.SetAttributes(configWtfPath, FileAttributes.ReadOnly);
                    }
                }
            }
            catch
            {
                AmeisenLogger.I.Log("StartWow", "Cannot write realmlist to config.wtf");
            }
        }

        private void CheckTosAndEula()
        {
            try
            {
                string configWtfPath = Path.Combine(Directory.GetParent(Config.PathToWowExe).FullName, "wtf", "config.wtf");

                if (File.Exists(configWtfPath))
                {
                    bool editedFile = false;
                    string content = File.ReadAllText(configWtfPath);

                    if (!content.Contains("SET READEULA \"0\"", StringComparison.OrdinalIgnoreCase))
                    {
                        editedFile = true;

                        if (content.Contains("SET READEULA", StringComparison.OrdinalIgnoreCase))
                        {
                            content = content.Replace("SET READEULA \"0\"", "SET READEULA \"1\"", StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            content += "\nSET READEULA \"1\"";
                        }
                    }

                    if (!content.Contains("SET READTOS \"0\"", StringComparison.OrdinalIgnoreCase))
                    {
                        editedFile = true;

                        if (content.Contains("SET READTOS", StringComparison.OrdinalIgnoreCase))
                        {
                            content = content.Replace("SET READTOS \"0\"", "SET READTOS \"1\"", StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            content += "\nSET READTOS \"1\"";
                        }
                    }

                    if (!content.Contains("SET MOVIE \"0\"", StringComparison.OrdinalIgnoreCase))
                    {
                        editedFile = true;

                        if (content.Contains("SET MOVIE", StringComparison.OrdinalIgnoreCase))
                        {
                            content = content.Replace("SET MOVIE \"0\"", "SET MOVIE \"1\"", StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            content += "\nSET MOVIE \"1\"";
                        }
                    }

                    if (editedFile)
                    {
                        File.SetAttributes(configWtfPath, FileAttributes.Normal);
                        File.WriteAllText(configWtfPath, content);
                        File.SetAttributes(configWtfPath, FileAttributes.ReadOnly);
                    }
                }
            }
            catch
            {
                AmeisenLogger.I.Log("StartWow", "Cannot write to config.wtf");
            }
        }
    }
}