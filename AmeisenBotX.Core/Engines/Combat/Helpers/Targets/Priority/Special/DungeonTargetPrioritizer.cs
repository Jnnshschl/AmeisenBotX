using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Priority.Special
{
    public class DungeonTargetPrioritizer : ITargetPrioritizer
    {
        public DungeonTargetPrioritizer(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            // add per map validation functions here, lambda should return true if the unit has
            // pririty, false if not
            Priorities = new()
            {
                { WowMapId.UtgardeKeep, UtgardeKeepIsIceblock },
            };
        }

        private AmeisenBotInterfaces Bot { get; }

        private Dictionary<WowMapId, Func<IWowUnit, bool>> Priorities { get; }

        public bool HasPriority(IWowUnit unit)
        {
            if (Priorities.TryGetValue(Bot.Objects.MapId, out Func<IWowUnit, bool> hasPriority))
            {
                return hasPriority(unit);
            }

            // no entry found, skip validation
            return false;
        }

        private bool UtgardeKeepIsIceblock(IWowUnit unit)
        {
            return Bot.Db.GetUnitName(unit, out string name) && name == "Frozen Tomb"; // TODO: find display id
        }
    }
}