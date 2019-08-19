using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Memory;
using AmeisenBotX.Memory.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateIdle : State
    {
        private AmeisenBotConfig Config { get; }
        private IOffsetList OffsetList { get; }
        private ObjectManager ObjectManager { get; }
        private HookManager HookManager { get; }
        private EventHookManager EventHookManager { get; }

        public StateIdle(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, IOffsetList offsetList, ObjectManager objectManager, HookManager hookManager, EventHookManager eventHookManager) : base(stateMachine)
        {
            Config = config;
            OffsetList = offsetList;
            ObjectManager = objectManager;
            HookManager = hookManager;
            EventHookManager = eventHookManager;
        }

        public override void Enter()
        {
            AmeisenBotStateMachine.XMemory.ReadString(OffsetList.PlayerName, Encoding.ASCII, out string playerName);
            AmeisenBotStateMachine.PlayerName = playerName;

            // first start
            if (!HookManager.IsWoWHooked)
            {
                HookManager.SetupEndsceneHook();
                EventHookManager.Start();

                if (Config.SaveWowWindowPosition) LoadWowWindowPosition();
                if (Config.SaveBotWindowPosition) LoadBotWindowPosition();
            }
        }

        public override void Execute()
        {
            if (IsUnitToFollowThere())
                AmeisenBotStateMachine.SetState(AmeisenBotState.Following);
        }

        public override void Exit()
        {
        }

        public bool IsUnitToFollowThere()
        {
            WowPlayer PlayerToFollow = null;

            // TODO: make this crap less redundant
            // check the specific character
            List<WowPlayer> wowPlayers = ObjectManager.WowObjects.OfType<WowPlayer>().ToList();
            if (wowPlayers.Count > 0)
            {
                if (Config.FollowSpecificCharacter)
                {
                    PlayerToFollow = wowPlayers.FirstOrDefault(p => p.Name == Config.SpecificCharacterToFollow);
                    PlayerToFollow = SkipIfOutOfRange(PlayerToFollow);
                }

                // check the group/raid leader
                if (PlayerToFollow == null && Config.FollowGroupLeader)
                {
                    PlayerToFollow = wowPlayers.FirstOrDefault(p => p.Guid == ObjectManager.PartyleaderGuid);
                    PlayerToFollow = SkipIfOutOfRange(PlayerToFollow);
                }

                // check the group members
                if (PlayerToFollow == null && Config.FollowGroupMembers)
                {
                    PlayerToFollow = wowPlayers.FirstOrDefault(p => ObjectManager.PartymemberGuids.Contains(p.Guid));
                    PlayerToFollow = SkipIfOutOfRange(PlayerToFollow);
                }
            }

            return PlayerToFollow != null;
        }

        private WowPlayer SkipIfOutOfRange(WowPlayer PlayerToFollow)
        {
            if (PlayerToFollow != null)
            {
                double distance = PlayerToFollow.Position.GetDistance(ObjectManager.Player.Position);
                if (UnitIsOutOfRange(distance))
                    PlayerToFollow = null;
            }

            return PlayerToFollow;
        }

        private bool UnitIsOutOfRange(double distance)
           => (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance);

        private void LoadWowWindowPosition()
        {
            string filepath = Path.Combine(Config.BotDataPath, $"wowpos_{AmeisenBotStateMachine.PlayerName}.json");
            if (File.Exists(filepath))
            {
                try
                {
                    Rect rect = JsonConvert.DeserializeObject<Rect>(File.ReadAllText(filepath));
                    XMemory.SetWindowPosition(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, rect);
                }
                catch { }
            }
        }

        private void LoadBotWindowPosition()
        {
            string filepath = Path.Combine(Config.BotDataPath, $"botpos_{AmeisenBotStateMachine.PlayerName}.json");
            if (File.Exists(filepath))
            {
                try
                {
                    Rect rect = JsonConvert.DeserializeObject<Rect>(File.ReadAllText(filepath));
                    XMemory.SetWindowPosition(Process.GetCurrentProcess().MainWindowHandle, rect);
                }
                catch { }
            }
        }
    }
}