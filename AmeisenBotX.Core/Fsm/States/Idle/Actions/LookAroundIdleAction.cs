using System;

namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public class LookAroundIdleAction : IIdleAction
    {
        public LookAroundIdleAction(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public int MaxCooldown => 18 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 11 * 1000;

        public int MinDuration => 0;

        public WowInterface WowInterface { get; }

        private Random Rnd { get; }

        public bool Enter()
        {
            return true;
        }

        public void Execute()
        {
            float modificationFactor = ((float)Rnd.NextDouble() - 0.5f) / ((float)Rnd.NextDouble() * 5.0f);
            WowInterface.HookManager.WowSetFacing(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Player.Rotation + modificationFactor);
        }
    }
}