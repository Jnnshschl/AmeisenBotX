using AmeisenBotX.Common.Storage;
using AmeisenBotX.Core.Engines.Tactic.Bosses.Naxxramas10;
using AmeisenBotX.Core.Engines.Tactic.Bosses.TheObsidianSanctum10;
using AmeisenBotX.Core.Engines.Tactic.Dungeon.ForgeOfSouls;
using AmeisenBotX.Core.Engines.Tactic.Dungeon.PitOfSaron;
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
                // wotlk dungeons
                {
                    WowMapId.TheForgeOfSouls,
                    new()
                    {
                        { 0, new BronjahmTactic(Bot) },
                        { 1, new DevourerOfSoulsTactic(Bot) }
                    }
                },
                {
                    WowMapId.PitOfSaron,
                    new() { { 0, new IckAndKrickTactic(Bot) } }
                },
                // wotlk raids
                {
                    WowMapId.Naxxramas,
                    new() { { 0, new AnubRekhan10Tactic(Bot) } }
                },
                {
                    WowMapId.TheObsidianSanctum,
                    new() { { 0, new TwilightPortalTactic(Bot) } }
                },
            };

            foreach (SortedList<int, ITactic> instances in Tactics.Values)
            {
                foreach (ITactic tactic in instances.Values)
                {
                    if (tactic is IStoreable s)
                    {
                        Bot.Storage.Register(s);
                    }
                }
            }
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
    }
}