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
        }

        private IBattlegroundProfile BattlegroundProfile { get; set; }

        private ObjectManager ObjectManager { get; set; }

        private HookManager HookManager { get; set; }

        private IMovementEngine MovementEngine { get; set; }

        public void Execute()
        {
            if (BattlegroundProfile == null)
            {
                TryLoadProfile(ObjectManager.MapId);
            }

            foreach (IBattlegroundObjective objective in BattlegroundProfile.Objectives.OrderByDescending(e => e.Priority))
            {
                if (!objective.IsAvailable)
                {
                    continue;
                }

                objective.Execute();
                return;
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
                    BattlegroundProfile = new WarsongGulchProfile(ObjectManager.Player.IsAlliance(), HookManager, ObjectManager, MovementEngine);
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
    }
}
