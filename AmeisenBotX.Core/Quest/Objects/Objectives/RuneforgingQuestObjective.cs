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

            EnchantEvent = new(TimeSpan.FromSeconds(1));
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private EnchantItemQuestObjectiveCondition Condition { get; }

        private TimegatedEvent EnchantEvent { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished || WowInterface.Player.IsCasting)
            {
                WowInterface.HookManager.LuaClickUiElement("TradeSkillFrameCloseButton"); return;
            }

            if (EnchantEvent.Run())
            {
                WowInterface.MovementEngine.Reset();
                WowInterface.HookManager.WowStopClickToMove();

                WowInterface.HookManager.LuaCastSpell("Runeforging");
                WowInterface.HookManager.LuaClickUiElement("TradeSkillCreateButton");
                WowInterface.HookManager.LuaUseInventoryItem(WowEquipmentSlot.INVSLOT_MAINHAND);
                WowInterface.HookManager.LuaClickUiElement("StaticPopup1Button1");
            }
        }
    }
}