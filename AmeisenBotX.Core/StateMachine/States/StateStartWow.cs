using AmeisenBotX.Logging;
using System;
using System.IO;

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
            CheckTosAndEula();

            if (File.Exists(Config.PathToWowExe))
            {
                WowInterface.WowProcess = WowInterface.XMemory.StartProcessNoActivate(Config.PathToWowExe);

                if (WowInterface.WowProcess != null)
                {
                    WowInterface.WowProcess.WaitForInputIdle();
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
                StateMachine.SetState((int)BotState.None);
                return;
            }

            if (Config.AutoLogin)
            {
                StateMachine.SetState((int)BotState.Login);
            }
            else
            {
                StateMachine.SetState((int)BotState.Idle);
            }
        }

        public override void Exit()
        {
        }

        private void CheckTosAndEula()
        {
            try
            {
                string configWtfPath = Path.Combine(Directory.GetParent(Config.PathToWowExe).FullName, "wtf", "config.wtf");
                if (File.Exists(configWtfPath))
                {
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
                }
            }
            catch
            {
                AmeisenLogger.Instance.Log("StartWow", "Cannot write to config.wtf, maybe its write protected");
            }
        }
    }
}