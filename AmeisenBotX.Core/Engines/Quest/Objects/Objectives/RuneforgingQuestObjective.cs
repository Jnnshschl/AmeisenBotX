using AmeisenBotX.Common.Utils;
using AmeisenBotX.Wow.Objects.Enums;
using System;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public delegate bool EnchantItemQuestObjectiveCondition();

    public class RuneforgingQuestObjective : IQuestObjective
    {
        public RuneforgingQuestObjective(AmeisenBotInterfaces bot, EnchantItemQuestObjectiveCondition condition)
        {
            Bot = bot;
            Condition = condition;

            EnchantEvent = new(TimeSpan.FromSeconds(1));
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; }

        private EnchantItemQuestObjectiveCondition Condition { get; }

        private TimegatedEvent EnchantEvent { get; }

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