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
                        { 1, new AnubRekhan10Tactic(Bot) }
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

        public bool AllowAttacking { get; private set; }

        public bool PreventMovement { get; private set; }

        private AmeisenBotInterfaces Bot { get; }

        private Dictionary<WowMapId, SortedList<int, ITactic>> Tactics { get; set; }

        public bool Execute()
        {
            if (Tactics.ContainsKey(Bot.Objects.MapId))
            {
                foreach (ITactic tactic in Tactics[Bot.Objects.MapId].Values)
                {
                    if (tactic.IsInArea(Bot.Player.Position) && tactic.ExecuteTactic(Bot.CombatClass.Role, Bot.CombatClass.IsMelee, out bool preventMovement, out bool allowAttacking))
                    {
                        PreventMovement = preventMovement;
                        AllowAttacking = allowAttacking;
                        return true;
                    }
                }
            }

            PreventMovement = false;
            AllowAttacking = true;
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