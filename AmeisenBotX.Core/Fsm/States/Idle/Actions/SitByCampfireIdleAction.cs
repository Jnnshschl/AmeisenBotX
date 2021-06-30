﻿using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public class SitByCampfireIdleAction : IIdleAction
    {
        public SitByCampfireIdleAction(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public int MaxCooldown => 15 * 60 * 1000;

        public int MaxDuration => 2 * 60 * 1000;

        public int MinCooldown => 12 * 60 * 1000;

        public int MinDuration => 1 * 60 * 1000;

        public AmeisenBotInterfaces Bot { get; }

        private bool PlacedCampfire { get; set; }

        private Random Rnd { get; }

        private bool SatDown { get; set; }

        public bool Enter()
        {
            PlacedCampfire = false;
            SatDown = false;

            return Bot.Character.SpellBook.IsSpellKnown("Basic Campfire");
        }

        public void Execute()
        {
            if (PlacedCampfire && SatDown)
            {
                return;
            }

            WowGameobject nearCampfire = Bot.Objects.WowObjects.OfType<WowGameobject>()
                .FirstOrDefault(e => e.DisplayId == (int)WowGameobjectDisplayId.CookingCampfire && Bot.Objects.PartymemberGuids.Contains(e.CreatedBy));

            if (nearCampfire != null && !SatDown)
            {
                Bot.Wow.WowFacePosition(Bot.Player.BaseAddress, Bot.Player.Position, nearCampfire.Position);
                Bot.Wow.LuaSendChatMessage(Rnd.Next(0, 2) == 1 ? "/sit" : "/sleep");
                SatDown = true;
            }
            else if (!PlacedCampfire)
            {
                Bot.Wow.LuaCastSpell("Basic Campfire");
                PlacedCampfire = true;
            }
        }
    }
}