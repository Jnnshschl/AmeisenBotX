using System;

namespace AmeisenBotX.Core.StateMachine.States.Idle.Actions
{
    public class LookAroundIdleAction : IIdleAction
    {
        public LookAroundIdleAction()
        {
            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public int MaxCooldown => 18 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 11 * 1000;

        public int MinDuration => 0;

        private Random Rnd { get; }

        public bool Enter()
        {
            return true;
        }

        public void Execute()
        {
            float modificationFactor = ((float)Rnd.NextDouble() - 0.5f) / ((float)Rnd.NextDouble() * 5.0f);
            WowInterface.I.HookManager.WowSetFacing(WowInterface.I.ObjectManager.Player, WowInterface.I.ObjectManager.Player.Rotation + modificationFactor);
        }
    }
}