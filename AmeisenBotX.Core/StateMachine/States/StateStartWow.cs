using AmeisenBotX.Logging;
using AmeisenBotX.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateStartWow : BasicState
    {
        public StateStartWow(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public event Action OnWoWStarted;

        public override void Enter()
        {
            AmeisenLogger.Instance.Log("StartWow", "Setting TOS and EULA to 1 in config");
            CheckTosAndEula();

            if (Config.AutoChangeRealmlist)
            {
                AmeisenLogger.Instance.Log("StartWow", "Changing Realmlist");
                ChangeRealmlist();
            }
        }

        public override void Execute()
        {
            if (!StateMachine.ShouldExit && File.Exists(Config.PathToWowExe))
            {
                if (WowInterface.WowProcess == null || WowInterface.WowProcess.HasExited)
                {
                    AmeisenLogger.Instance.Log("StartWow", "Starting WoW Process");
                    WowInterface.WowProcess = XMemory.StartProcessNoActivate(Config.PathToWowExe);

                    AmeisenLogger.Instance.Log("StartWow", "Waiting for input idle");
                    WowInterface.WowProcess.WaitForInputIdle();
                }

                if (WowInterface.WowProcess != null && !WowInterface.WowProcess.HasExited)
                {
                    AmeisenLogger.Instance.Log("StartWow", $"Attaching XMemory to {WowInterface.WowProcess.ProcessName}:{WowInterface.WowProcess.Id}");

                    try
                    {
                        if (WowInterface.XMemory.Attach(WowInterface.WowProcess))
                        {
                            try
                            {
                                OnWoWStarted?.Invoke();
                            }
                            catch (Exception ex) { AmeisenLogger.Instance.Log("StartWow", $"Error at OnWoWStarted:\n{ex}"); }

                            AmeisenLogger.Instance.Log("StartWow", $"Switching to login state...");
                            StateMachine.SetState(BotState.Login);
                        }
                        else
                        {
                            AmeisenLogger.Instance.Log("StartWow", $"Attaching XMemory failed...");
                        }
                    }
                    catch (Exception e) { AmeisenLogger.Instance.Log("StartWow", $"Attaching XMemory failed:\n{e}"); }
                }
            }
        }

        public override void Exit()
        {
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
                AmeisenLogger.Instance.Log("StartWow", "Cannot write realmlist to config.wtf");
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
                AmeisenLogger.Instance.Log("StartWow", "Cannot write to config.wtf");
            }
        }
    }
}