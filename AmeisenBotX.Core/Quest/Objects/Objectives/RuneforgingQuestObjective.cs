using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Character.Inventory.Enums;
using System;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
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
                Bot.Wow.LuaClickUiElement("TradeSkillFrameCloseButton"); return;
            }

            if (EnchantEvent.Run())
            {
                Bot.Movement.Reset();
                Bot.Wow.WowStopClickToMove();

                Bot.Wow.LuaCastSpell("Runeforging");
                Bot.Wow.LuaClickUiElement("TradeSkillCreateButton");
                Bot.Wow.LuaUseInventoryItem(WowEquipmentSlot.INVSLOT_MAINHAND);
                Bot.Wow.LuaClickUiElement("StaticPopup1Button1");
            }
        }
    }
}