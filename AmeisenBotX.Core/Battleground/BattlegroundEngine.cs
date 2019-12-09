using AmeisenBotX.Core.Battleground.Objectives;
using AmeisenBotX.Core.Battleground.Profiles;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Battleground
{
    public class BattlegroundEngine
    {
        public BattlegroundEngine(HookManager hookManager, ObjectManager objectManager, IMovementEngine movementEngine)
        {
            HookManager = hookManager;
            ObjectManager = objectManager;
            MovementEngine = movementEngine;

            ForceCombat = false;
            CurrentObjectiveName = "None";
        }

        public string CurrentObjectiveName { get; private set; }

        public bool ForceCombat { get; set; }

        private IBattlegroundProfile BattlegroundProfile { get; set; }

        private ObjectManager ObjectManager { get; set; }

        private HookManager HookManager { get; set; }

        private IMovementEngine MovementEngine { get; set; }

        private IBattlegroundObjective LastObjective { get; set; }

        public void Execute()
        {
            if (BattlegroundProfile == null)
            {
                TryLoadProfile(ObjectManager.MapId);
            }

            if (BattlegroundProfile != null)
            {
                foreach (IBattlegroundObjective objective in BattlegroundProfile.Objectives.OrderByDescending(e => e.Priority))
                {
                    if (!objective.IsAvailable)
                    {
                        continue;
                    }

                    if(CurrentObjectiveName != objective.GetType().Name)
                    {
                        LastObjective?.Exit();
                        objective.Enter();
                    }

                    objective.Execute();
                    CurrentObjectiveName = objective.GetType().Name;
                    return;
                }
            }
        }

        public bool TryLoadProfile(int mapId)
        {
            switch (mapId)
            {
                case 30:
                    // Alterac Valley
                    return false;

                case 489:
                    BattlegroundProfile = new WarsongGulchProfile(ObjectManager.Player.IsAlliance(), HookManager, ObjectManager, MovementEngine, ForceCombat);
                    return true;

                case 529:
                    // Arathi Basin
                    return false;

                case 566:
                    // Eye of the Storm
                    return false;

                case 607:
                    // Strand of the Ancients
                    return false;

                default:
                    return false;
            }
        }

        public void AllianceFlagWasPickedUp(string playername)
        {
            if (BattlegroundProfile.GetType() == typeof(WarsongGulchProfile))
            {
                ((WarsongGulchProfile)BattlegroundProfile).AllianceFlagWasPickedUp(playername);
            }
        }

        public void HordeFlagWasPickedUp(string playername)
        {
            if (BattlegroundProfile.GetType() == typeof(WarsongGulchProfile))
            {
                ((WarsongGulchProfile)BattlegroundProfile).HordeFlagWasPickedUp(playername);
            }
        }

        public void AllianceFlagWasDropped(string playername)
        {
            if (BattlegroundProfile.GetType() == typeof(WarsongGulchProfile))
            {
                ((WarsongGulchProfile)BattlegroundProfile).AllianceFlagWasDropped(playername);
            }
        }

        public void HordeFlagWasDropped(string playername)
        {
            if (BattlegroundProfile.GetType() == typeof(WarsongGulchProfile))
            {
                ((WarsongGulchProfile)BattlegroundProfile).HordeFlagWasDropped(playername);
            }
        }
    }
}
