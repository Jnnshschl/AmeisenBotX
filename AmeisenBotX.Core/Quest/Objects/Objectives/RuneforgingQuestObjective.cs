using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
using System;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool EnchantItemQuestObjectiveCondition();

    public class RuneforgingQuestObjective : IQuestObjective
    {
        public RuneforgingQuestObjective(WowInterface wowInterface, EnchantItemQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
            Condition = condition;

            EnchantEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private EnchantItemQuestObjectiveCondition Condition { get; }

        private WowInterface WowInterface { get; }

        private TimegatedEvent EnchantEvent { get; }

        public void Execute()
        {
            if (Finished || WowInterface.ObjectManager.Player.IsCasting)
            {
                WowInterface.HookManager.ClickUiElement("TradeSkillFrameCloseButton"); return;
            }

            if (EnchantEvent.Run())
            {
                WowInterface.MovementEngine.Reset();
                WowInterface.HookManager.StopClickToMoveIfActive();

                WowInterface.HookManager.CastSpell("Runeforging");
                WowInterface.HookManager.ClickUiElement("TradeSkillCreateButton");
                WowInterface.HookManager.UseInventoryItem(EquipmentSlot.INVSLOT_MAINHAND);
                WowInterface.HookManager.ClickUiElement("StaticPopup1Button1");
            }
        }
    }
}