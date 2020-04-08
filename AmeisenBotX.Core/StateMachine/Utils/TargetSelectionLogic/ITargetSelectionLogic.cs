using AmeisenBotX.Core.Data.Objects.WowObject;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic
{
    public interface ITargetSelectionLogic
    {
        void Reset();

        bool SelectTarget(out List<WowUnit> wowUnit);
    }
}