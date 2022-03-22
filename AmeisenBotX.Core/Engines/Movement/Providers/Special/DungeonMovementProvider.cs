using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Engines.Movement.Providers.Basic;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Movement.Providers.Special
{
    public class DungeonMovementProvider : IMovementProvider
    {
        public DungeonMovementProvider(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            Providers = new()
            {
                { WowMapId.TempleOfTheJadeSerpent, TempleOfTheJadeSerpent },
            };

            MariMovementProvider = new StayAroundMovementProvider(() => (Bot.Target, MathF.PI * 0.75f, Bot.CombatClass == null || Bot.CombatClass.IsMelee ? Bot.Player.MeleeRangeTo(Bot.Target) : 7.5f));
        }

        private AmeisenBotInterfaces Bot { get; }

        private StayAroundMovementProvider MariMovementProvider { get; }

        private Dictionary<WowMapId, Func<IMovementProvider>> Providers { get; }

        public bool Get(out Vector3 position, out MovementAction type)
        {
            if (Providers.TryGetValue(Bot.Objects.MapId, out Func<IMovementProvider> getProvider))
            {
                IMovementProvider provider = getProvider();

                if (provider != null)
                {
                    return provider.Get(out position, out type);
                }
            }

            type = MovementAction.None;
            position = Vector3.Zero;
            return false;
        }

        private IMovementProvider TempleOfTheJadeSerpent()
        {
            if (Bot.Objects.All.OfType<IWowUnit>().Any(e => e.CurrentlyCastingSpellId == 106055 || e.CurrentlyChannelingSpellId == 106055))
            {
                // dodge wise mari hydrolance
                return MariMovementProvider;
            }

            return null;
        }
    }
}