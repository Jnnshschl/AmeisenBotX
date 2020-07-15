using AmeisenBotX.Logging;
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

        private DateTime WowStart { get; set; }

        public override void Enter()
        {
            AmeisenLogger.Instance.Log("StartWow", "Setting TOS and EULA to 1 in config");
            CheckTosAndEula();

            if (Config.AutoChangeRealmlist)
            {
                AmeisenLogger.Instance.Log("StartWow", "Changing Realmlist");
                ChangeRealmlist();
            }

            if (!StateMachine.ShouldExit && File.Exists(Config.PathToWowExe))
            {
                if (WowInterface.WowProcess == null || WowInterface.WowProcess.HasExited)
                {
                    AmeisenLogger.Instance.Log("StartWow", "Starting WoW Process");
                    WowInterface.WowProcess = WowInterface.XMemory.StartProcessNoActivate(Config.PathToWowExe);

                    AmeisenLogger.Instance.Log("StartWow", "Waiting for input idle");
                    WowInterface.WowProcess.WaitForInputIdle();

                    AmeisenLogger.Instance.Log("StartWow", "Attaching XMemory");
                    WowInterface.XMemory.Attach(WowInterface.WowProcess);

                    WowStart = DateTime.Now;
                    OnWoWStarted?.Invoke();
                }
            }
        }

        public override void Execute()
        {
            if (DateTime.Now - WowStart > TimeSpan.FromSeconds(8) && WowInterface.WowProcess.HasExited)
            {
                StateMachine.SetState(BotState.None);
                return;
            }

            if (Config.AutoLogin)
            {
                StateMachine.SetState(BotState.Login);
            }
            else
            {
                StateMachine.SetState(BotState.Idle);
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
                    File.SetAttributes(configWtfPath, FileAttributes.Normal);
                    List<string> content = File.ReadAllLines(configWtfPath).ToList();

                    bool found = false;
                    for (int i = 0; i < content.Count; i++)
                    {
                        if (content[i].ToUpper().Contains("SET REALMLIST"))
                        {
                            content[i] = $"SET REALMLIST {Config.Realmlist}";
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        content.Add($"SET REALMLIST {Config.Realmlist}");
                    }

                    File.WriteAllLines(configWtfPath, content);
                    File.SetAttributes(configWtfPath, FileAttributes.ReadOnly);
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
                    File.SetAttributes(configWtfPath, FileAttributes.Normal);
                    string content = File.ReadAllText(configWtfPath).ToUpper();

                    if (content.Contains("SET READEULA"))
                    {
                        content = content.Replace("SET READEULA \"0\"", "SET READEULA \"1\"");
                    }
                    else
                    {
                        content += "\nSET READEULA \"1\"";
                    }

                    if (content.Contains("SET READTOS"))
                    {
                        content = content.Replace("SET READTOS \"0\"", "SET READTOS \"1\"");
                    }
                    else
                    {
                        content += "\nSET READTOS \"1\"";
                    }

                    File.WriteAllText(configWtfPath, content);
                    File.SetAttributes(configWtfPath, FileAttributes.ReadOnly);
                }
            }
            catch
            {
                AmeisenLogger.Instance.Log("StartWow", "Cannot write to config.wtf");
            }
        }
    }
}