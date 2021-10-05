using AmeisenBotX.Core.Engines.Tactic.Bosses.Naxxramas10;
using AmeisenBotX.Core.Engines.Tactic.Bosses.TheObsidianSanctum10;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Tactic
{
    public class DefaultTacticEngine : ITacticEngine
    {
        public DefaultTacticEngine(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            Tactics = new()
            {
                {
                    WowMapId.Naxxramas,
                    new()
                    {
                        { 1, new AnubRhekan10Tactic(Bot) }
                    }
                },
                {
                    WowMapId.TheObsidianSanctum,
                    new()
                    {
                        { 1, new TwilightPortalTactic(Bot) }
                    }
                },
            };
        }

        private AmeisenBotInterfaces Bot { get; }

        private Dictionary<WowMapId, SortedList<int, ITactic>> Tactics { get; set; }

        public bool Execute(out bool preventMovement, out bool allowAttacking)
        {
            foreach (ITactic tactic in Tactics[Bot.Objects.MapId].Values)
            {
                if (tactic.IsInArea(Bot.Player.Position) && tactic.ExecuteTactic(Bot.CombatClass.Role, Bot.CombatClass.IsMelee, out preventMovement, out allowAttacking))
                {
                    return true;
                }
            }

            preventMovement = false;
            allowAttacking = true;
            return false;
        }

        public bool HasTactics()
        {
            return Tactics.Count > 0;
        }

        public void Reset()
        {
            Tactics.Clear();
        }
    }
}