using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Managers.Threat
{
    /// <summary>
    /// Manager to observe environmental threats (not threat of mobs)
    /// </summary>
    public class ThreatManager
    {
        public ThreatManager(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            Bot = bot;
            Config = config;
        }

        private AmeisenBotInterfaces Bot { get; }

        private AmeisenBotConfig Config { get; }

        /// <summary>
        /// This method tries to calculate how dangerous a position is for us.
        /// 
        /// Range: 0.0f (Nothing) -> 100.0f (Deadly)
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>Level of danger</returns>
        public float Get(Vector3 position)
        {
            const float unitThreat = 33.0f;
            const float unitAggroDistance = 8.0f;
            const float playerThreat = 70.0f;

            float threat = 0.0f;

            // hostile players
            IEnumerable<IWowUnit> hostilePlayers = Bot.Objects.WowObjects.OfType<IWowPlayer>().Where
            (
                e => e.Type == WowObjectType.Player
                && !e.IsDead
                && Bot.Wow.GetReaction(e.BaseAddress, Bot.Player.BaseAddress) == WowUnitReaction.Hostile
                && e.DistanceTo(position) < 60.0f
            );

            if (hostilePlayers.Any())
            {
                foreach (IWowPlayer player in hostilePlayers)
                {
                    float leveldiffMult = MathF.Max(0.0f, MathF.Tanh(player.Level - Bot.Player.Level));

                    // players are a big threat to us
                    threat += playerThreat * leveldiffMult;

                    if (threat >= 100.0f)
                    {
                        return threat;
                    }
                }
            }

            // hostile npcs
            IEnumerable<IWowUnit> hostileUnits = Bot.Objects.WowObjects.OfType<IWowUnit>().Where
            (
                e => e.Type == WowObjectType.Unit
                && !e.IsDead
                && Bot.Wow.GetReaction(e.BaseAddress, Bot.Player.BaseAddress) == WowUnitReaction.Hostile
                && e.DistanceTo(position) < 50.0f
            );

            if (hostileUnits.Any())
            {
                foreach (IWowUnit unit in hostileUnits)
                {
                    // TODO: handle elites, handle level 80 good gear scaling
                    float leveldiffMult = MathF.Max(0.0f, MathF.Tanh(unit.Level - Bot.Player.Level));

                    if (leveldiffMult == 0.0f)
                    {
                        // npc is far beyond our level
                        continue;
                    }

                    float aggroRange = Bot.Player.AggroRangeTo(unit);
                    float distance = unit.DistanceTo(position);

                    if (distance < aggroRange)
                    {
                        // more than 3 units will get us in trouble, we should avoid that
                        threat += unitThreat * leveldiffMult;
                    }
                    else
                    {
                        // measure how close we are until aggro
                        float distanceUntilAggro = distance - aggroRange;

                        if (distanceUntilAggro < unitAggroDistance)
                        {
                            threat += unitThreat * leveldiffMult * (distanceUntilAggro / unitAggroDistance);
                        }
                    }

                    if (threat >= 100.0f)
                    {
                        return threat;
                    }
                }
            }

            return threat;
        }
    }
}
