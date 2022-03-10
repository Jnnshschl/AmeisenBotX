using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using System;

namespace AmeisenBotX.Core.Logic.Routines
{
    public static class SpeakToClassTrainerRoutine
    {
        public static bool Run(AmeisenBotInterfaces bot, IWowUnit selectedUnit)
        {
            if (bot == null || selectedUnit == null)
            {
                return false;
            }

            if (bot.Wow.TargetGuid != selectedUnit.Guid)
            {
                bot.Wow.ChangeTarget(selectedUnit.Guid);
            }

            if (!BotMath.IsFacing(bot.Objects.Player.Position, bot.Objects.Player.Rotation, selectedUnit.Position, 0.5f))
            {
                bot.Wow.FacePosition(bot.Objects.Player.BaseAddress, bot.Player.Position, selectedUnit.Position);
            }

            if (!bot.Wow.UiIsVisible("GossipFrame"))
            {
                bot.Wow.InteractWithUnit(selectedUnit);
            }

            if (!selectedUnit.IsGossip)
            {
                return false;
            }

            // gossip 1 train skills
            // gossip 2 unlearn talents
            // quest gossip from trainer??

            string[] gossipTypes = bot.Wow.GetGossipTypes();

            for (int i = 0; i < gossipTypes.Length; ++i)
            {
                if (!gossipTypes[i].Equals("trainer", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // +1 is due to implicit conversion between lua array (indexed at 1 not 0) and c# array
                bot.Wow.SelectGossipOptionSimple(i + 1);
                return true;
            }

            return false;
        }
    }
}