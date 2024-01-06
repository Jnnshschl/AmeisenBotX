using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Basic;
using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation
{
    public class TargetValidationManager : ITargetValidator
    {
        public TargetValidationManager(ITargetValidator validator)
        {
            Validators = [validator];
            BlacklistTargetValidator = new();
        }

        public TargetValidationManager(IEnumerable<ITargetValidator> validators)
        {
            Validators = new(validators);
            BlacklistTargetValidator = new();
        }

        public DisplayIdBlacklistTargetValidator BlacklistTargetValidator { get; }

        public List<ITargetValidator> Validators { get; }

        public void Add(ITargetValidator validator)
        {
            Validators.Add(validator);
        }

        public bool IsValid(IWowUnit unit)
        {
            // is unit on blacklist
            return BlacklistTargetValidator.IsValid(unit)
                // run all other validators
                && Validators.All(e => e.IsValid(unit));
        }
    }
}