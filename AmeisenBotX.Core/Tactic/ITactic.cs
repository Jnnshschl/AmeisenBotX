using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Core.Tactic
{
    public interface ITactic
    {
        bool ExecuteTactic(CombatClassRole role, bool isMelee, out bool handlesMovement, out bool allowAttacking);
    }
}
