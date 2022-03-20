using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Special
{
    public class DungeonTargetValidator : ITargetValidator
    {
        public DungeonTargetValidator(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            // add per map validation functions here, lambda should return true if the unit is
            // invalid, false if its valid
            Validations = new()
            {
                { WowMapId.HallsOfReflection, HallsOfReflectionIsTheLichKing },
                { WowMapId.DrakTharonKeep, DrakTharonKeepIsNovosChanneling },
                { WowMapId.ThroneOfTides, ThroneOfTidesIsLadyNazjarChanneling },
                { WowMapId.TempleOfTheJadeSerpent, TempleOfTheJadeSerpent }
            };
        }

        private bool TempleOfTheJadeSerpent(IWowUnit arg)
        {
            return arg.Auras.Any(e => e.SpellId == 113315 && e.StackCount >= 2); // Peril and Strafe
        }

        private AmeisenBotInterfaces Bot { get; }

        private Dictionary<WowMapId, Func<IWowUnit, bool>> Validations { get; }

        public bool IsValid(IWowUnit unit)
        {
            if (Validations.TryGetValue(Bot.Objects.MapId, out Func<IWowUnit, bool> isInvalid))
            {
                return !isInvalid(unit);
            }

            // no entry found, skip validation
            return true;
        }

        private bool DrakTharonKeepIsNovosChanneling(IWowUnit unit)
        {
            return unit.CurrentlyChannelingSpellId == 47346;
        }

        private bool HallsOfReflectionIsTheLichKing(IWowUnit unit)
        {
            return Bot.Db.GetUnitName(unit, out string name) && name == "The Lich King";
        }

        private bool ThroneOfTidesIsLadyNazjarChanneling(IWowUnit unit)
        {
            return unit.CurrentlyChannelingSpellId == 75683;
        }
    }
}