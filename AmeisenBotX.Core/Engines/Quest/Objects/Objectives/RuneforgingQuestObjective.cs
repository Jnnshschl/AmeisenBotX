using AmeisenBotX.Common.Utils;
using AmeisenBotX.Wow.Objects.Enums;
using System;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public class RuneforgingQuestObjective(AmeisenBotInterfaces bot, Func<bool> condition) : IQuestObjective
    {
        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; } = bot;

        private Func<bool> Condition { get; } = condition;

        private TimegatedEvent EnchantEvent { get; } = new(TimeSpan.FromSeconds(1));

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting)
            {
                Bot.Wow.ClickUiElement("TradeSkillFrameCloseButton");
                return;
            }

            if (EnchantEvent.Run())
            {
                Bot.Movement.Reset();
                Bot.Wow.StopClickToMove();

                Bot.Wow.CastSpell("Runeforging");
                Bot.Wow.ClickUiElement("TradeSkillCreateButton");
                Bot.Wow.UseInventoryItem(WowEquipmentSlot.INVSLOT_MAINHAND);
                Bot.Wow.ClickUiElement("StaticPopup1Button1");
            }
        }
    }
}