using System;

namespace AmeisenBotX.Core.Logic.Idle.Actions
{
    public class OpenMapIdleAction : IIdleAction
    {
        public OpenMapIdleAction(AmeisenBotInterfaces bot)
        {
            Bot = bot;
        }

        public bool AutopilotOnly => false;

        public AmeisenBotInterfaces Bot { get; }

        public DateTime Cooldown { get; set; }

        public int MaxCooldown => 13 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 7 * 1000;

        public int MinDuration => 0;

        public bool Enter()
        {
            return true;
        }

        public void Execute()
        {
            // open map and make the windows small and transparent
            Bot.Wow.LuaDoString(@"
                if WorldMapFrame:IsShown() then 
                    WorldMapFrame:Hide() 
                    WorldMapFrame:SetAlpha(1.0) 
                else WorldMapFrame:Show() 
                    if WorldMapFrameSizeDownButton then 
                        WorldMapFrameSizeDownButton:Click() 
                    end 

                    WorldMapFrame:SetAlpha(0.1) 
                end");
        }

        public override string ToString()
        {
            return $"{(AutopilotOnly ? "(🤖) " : "")}Open Map";
        }
    }
}
