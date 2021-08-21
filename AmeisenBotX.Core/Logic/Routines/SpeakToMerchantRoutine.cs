using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using System;

namespace AmeisenBotX.Core.Logic.Routines
{
    public static class SpeakToMerchantRoutine
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
                return false;
            }

            if (!BotMath.IsFacing(bot.Objects.Player.Position, bot.Objects.Player.Rotation, selectedUnit.Position, 0.5f))
            {
                bot.Wow.FacePosition(bot.Objects.Player.BaseAddress, bot.Player.Position, selectedUnit.Position);
            }

            if (!bot.Wow.UiIsVisible("GossipFrame", "MerchantFrame"))
            {
                bot.Wow.InteractWithUnit(selectedUnit.BaseAddress);
                return false;
            }

            if (selectedUnit.IsGossip)
            {
                if (bot.Wow.UiIsVisible("GossipFrame"))
                {
                    string[] gossipTypes = bot.Wow.GetGossipTypes();

                    for (int i = 0; i < gossipTypes.Length; ++i)
                    {
                        if (gossipTypes[i].Equals("vendor", StringComparison.OrdinalIgnoreCase)
                            || gossipTypes[i].Equals("repair", StringComparison.OrdinalIgnoreCase))
                        {
                            bot.Wow.SelectGossipOption(i + 1);
                        }
                    }
                }

                if (!bot.Wow.UiIsVisible("MerchantFrame"))
                {
                    return false;
                }
            }

            return true;
        }
    }
}