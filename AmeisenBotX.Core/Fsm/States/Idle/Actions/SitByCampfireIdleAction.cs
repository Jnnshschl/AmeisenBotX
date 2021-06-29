using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Data.Objects;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public class SitByCampfireIdleAction : IIdleAction
    {
        public SitByCampfireIdleAction(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public int MaxCooldown => 15 * 60 * 1000;

        public int MaxDuration => 2 * 60 * 1000;

        public int MinCooldown => 12 * 60 * 1000;

        public int MinDuration => 1 * 60 * 1000;

        public WowInterface WowInterface { get; }

        private bool PlacedCampfire { get; set; }

        private Random Rnd { get; }

        private bool SatDown { get; set; }

        public bool Enter()
        {
            PlacedCampfire = false;
            SatDown = false;

            return WowInterface.CharacterManager.SpellBook.IsSpellKnown("Basic Campfire");
        }

        public void Execute()
        {
            if (PlacedCampfire && SatDown)
            {
                return;
            }

            WowGameobject nearCampfire = WowInterface.Objects.WowObjects.OfType<WowGameobject>()
                .FirstOrDefault(e => e.DisplayId == (int)WowGameobjectDisplayId.CookingCampfire && WowInterface.Objects.PartymemberGuids.Contains(e.CreatedBy));

            if (nearCampfire != null && !SatDown)
            {
                WowInterface.NewWowInterface.WowFacePosition(WowInterface.Player.BaseAddress, WowInterface.Player.Position, nearCampfire.Position);
                WowInterface.NewWowInterface.LuaSendChatMessage(Rnd.Next(0, 2) == 1 ? "/sit" : "/sleep");
                SatDown = true;
            }
            else if (!PlacedCampfire)
            {
                WowInterface.NewWowInterface.LuaCastSpell("Basic Campfire");
                PlacedCampfire = true;
            }
        }
    }
}