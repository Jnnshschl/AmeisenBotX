using System;

namespace AmeisenBotX.Core.Logic.Idle.Actions
{
    public class SheathWeaponIdleAction : IIdleAction
    {
        public SheathWeaponIdleAction(AmeisenBotInterfaces bot)
        {
            Bot = bot;
        }

        public bool AutopilotOnly => false;

        public AmeisenBotInterfaces Bot { get; }

        public DateTime Cooldown { get; set; }

        public int MaxCooldown => 49 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 16 * 1000;

        public int MinDuration => 0;

        public bool Enter()
        {
            return true;
        }

        public void Execute()
        {
            Bot.Wow.LuaDoString("ToggleSheath()");
        }

        public override string ToString()
        {
            return $"{(AutopilotOnly ? "(🤖) " : "")}Sheath Weapon";
        }
    }
}