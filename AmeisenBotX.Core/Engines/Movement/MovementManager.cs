using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Movement
{
    public class MovementManager
    {
        public MovementManager(IEnumerable<IMovementProvider> providers)
        {
            Providers = providers;
        }

        public IEnumerable<IMovementProvider> Providers { get; set; }

        public Vector3 Target { get; private set; }

        public MovementAction Type { get; private set; }

        public bool NeedToMove()
        {
            foreach (IMovementProvider provider in Providers)
            {
                if (provider.Get(out Vector3 position, out MovementAction type))
                {
                    Target = position;
                    Type = type;
                    return true;
                }
            }

            Type = MovementAction.None;
            Target = Vector3.Zero;
            return false;
        }
    }
}