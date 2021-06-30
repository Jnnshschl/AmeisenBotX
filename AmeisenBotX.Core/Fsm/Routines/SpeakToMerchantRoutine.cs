using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Data.Objects;
using System;

namespace AmeisenBotX.Core.Fsm.Routines
{
    public static class SpeakToMerchantRoutine
    {
        public static bool Run(AmeisenBotInterfaces bot, WowUnit selectedUnit)
        {
            if (bot == null || selectedUnit == null)
            {
                return false;
            }

            if (bot.Wow.TargetGuid != selectedUnit.Guid)
            {
                bot.Wow.WowTargetGuid(selectedUnit.Guid);
                return false;
            }

            if (!BotMath.IsFacing(bot.Objects.Player.Position, bot.Objects.Player.Rotation, selectedUnit.Position))
            {
                bot.Wow.WowFacePosition(bot.Objects.Player.BaseAddress, bot.Player.Position, selectedUnit.Position);
            }

            if (selectedUnit.IsGossip)
            {
                if (bot.Wow.LuaUiIsVisible("GossipFrame"))
                {
                    string[] gossipTypes = bot.Wow.LuaGetGossipTypes();

                    for (int i = 0; i < gossipTypes.Length; ++i)
                    {
                        if (gossipTypes[i].Equals("vendor", StringComparison.OrdinalIgnoreCase)
                            || gossipTypes[i].Equals("repair", StringComparison.OrdinalIgnoreCase))
                        {
                            bot.Wow.LuaSelectGossipOption(i + 1);
                        }
                    }
                }

                if (!bot.Wow.LuaUiIsVisible("MerchantFrame"))
                {
                    return false;
                }
            }

            if (!bot.Wow.LuaUiIsVisible("GossipFrame", "MerchantFrame"))
            {
                bot.Wow.WowUnitRightClick(selectedUnit.BaseAddress);
                return false;
            }

            return true;
        }
    }
}