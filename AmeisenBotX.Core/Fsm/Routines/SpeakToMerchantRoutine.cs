using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Data.Objects;
using System;

namespace AmeisenBotX.Core.Fsm.Routines
{
    public static class SpeakToMerchantRoutine
    {
        public static bool Run(WowInterface wowInterface, WowUnit selectedUnit)
        {
            if (wowInterface == null || selectedUnit == null)
            {
                return false;
            }

            if (wowInterface.Objects.Target.Guid != selectedUnit.Guid)
            {
                wowInterface.NewWowInterface.WowTargetGuid(selectedUnit.Guid);
                return false;
            }

            if (!BotMath.IsFacing(wowInterface.Objects.Player.Position, wowInterface.Objects.Player.Rotation, selectedUnit.Position))
            {
                wowInterface.NewWowInterface.WowFacePosition(wowInterface.Objects.Player.BaseAddress, wowInterface.Player.Position, selectedUnit.Position);
            }

            if (selectedUnit.IsGossip)
            {
                if (wowInterface.NewWowInterface.LuaUiIsVisible("GossipFrame"))
                {
                    string[] gossipTypes = wowInterface.NewWowInterface.LuaGetGossipTypes();

                    for (int i = 0; i < gossipTypes.Length; ++i)
                    {
                        if (gossipTypes[i].Equals("vendor", StringComparison.OrdinalIgnoreCase)
                            || gossipTypes[i].Equals("repair", StringComparison.OrdinalIgnoreCase))
                        {
                            wowInterface.NewWowInterface.LuaSelectGossipOption(i + 1);
                        }
                    }
                }

                if (!wowInterface.NewWowInterface.LuaUiIsVisible("MerchantFrame"))
                {
                    return false;
                }
            }

            if (!wowInterface.NewWowInterface.LuaUiIsVisible("GossipFrame", "MerchantFrame"))
            {
                wowInterface.NewWowInterface.WowUnitRightClick(selectedUnit.BaseAddress);
                return false;
            }

            return true;
        }
    }
}