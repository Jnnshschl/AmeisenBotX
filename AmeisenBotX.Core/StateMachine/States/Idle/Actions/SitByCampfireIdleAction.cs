using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using System;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.States.Idle.Actions
{
    public class SitByCampfireIdleAction : IIdleAction
    {
        public SitByCampfireIdleAction()
        {
            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public int MaxCooldown => 15 * 60 * 1000;

        public int MaxDuration => 2 * 60 * 1000;

        public int MinCooldown => 12 * 60 * 1000;

        public int MinDuration => 1 * 60 * 1000;

        private bool PlacedCampfire { get; set; }

        private Random Rnd { get; }

        private bool SatDown { get; set; }

        public bool Enter()
        {
            PlacedCampfire = false;
            SatDown = false;

            return WowInterface.I.CharacterManager.SpellBook.IsSpellKnown("Basic Campfire");
        }

        public void Execute()
        {
            if (PlacedCampfire && SatDown)
            {
                return;
            }

            WowGameobject nearCampfire = WowInterface.I.ObjectManager.WowObjects.OfType<WowGameobject>()
                .FirstOrDefault(e => e.DisplayId == (int)GameobjectDisplayId.CookingCampfire && WowInterface.I.ObjectManager.PartymemberGuids.Contains(e.CreatedBy));

            if (nearCampfire != null && !SatDown)
            {
                WowInterface.I.HookManager.WowFacePosition(WowInterface.I.ObjectManager.Player, nearCampfire.Position);
                WowInterface.I.HookManager.LuaSendChatMessage(Rnd.Next(0, 2) == 1 ? "/sit" : "/sleep");
                SatDown = true;
            }
            else if (!PlacedCampfire)
            {
                WowInterface.I.HookManager.LuaCastSpell("Basic Campfire");
                PlacedCampfire = true;
            }
        }
    }
}