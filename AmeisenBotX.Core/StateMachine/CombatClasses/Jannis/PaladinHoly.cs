using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.StateMachine.Utils.AuraManager;
using static AmeisenBotX.Core.StateMachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class PaladinHoly : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string blessingOfWisdomSpell = "Blessing of Wisdom";
        private readonly string devotionAuraSpell = "Devotion Aura";
        private readonly string divineFavorSpell = "Divine Favor";
        private readonly string divineIlluminationSpell = "Divine Illumination";
        private readonly string divinePleaSpell = "Divine Plea";
        private readonly string flashOfLightSpell = "Flash of Light";
        private readonly string holyLightSpell = "Holy Light";
        private readonly string holyShockSpell = "Holy Shock";
        private readonly string layOnHandsSpell = "Lay on Hands";
        
        public PaladinHoly(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager) : base(objectManager, characterManager, hookManager)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { blessingOfWisdomSpell, () =>
                    {
                        HookManager.TargetGuid(ObjectManager.PlayerGuid);
                        return CastSpellIfPossible(blessingOfWisdomSpell, true);
                    }
                },
                { devotionAuraSpell, () => CastSpellIfPossible(devotionAuraSpell, true) }
            };

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, flashOfLightSpell },
                { 2000, holyShockSpell },
                { 10000, holyLightSpell }
            };
        }

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => true;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicSpiritComparator();

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override string Displayname => "Paladin Holy";

        public override string Version => "1.0";

        public override string Author => "Jannis";

        public override string Description => "FCFS based CombatClass for the Holy Paladin spec.";

        public override WowClass Class => WowClass.Paladin;

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (ObjectManager.Player.ManaPercentage < 80
                && CastSpellIfPossible(divinePleaSpell, true))
            {
                return;
            }

            if (NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing))
            {
                HandleTargetSelection(playersThatNeedHealing);
                ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);

                if (ObjectManager.Target != null)
                {
                    ObjectManager.UpdateObject(ObjectManager.Target.Type, ObjectManager.Target.BaseAddress);

                    if (ObjectManager.Target.HealthPercentage < 12
                        && CastSpellIfPossible(layOnHandsSpell))
                    {
                        return;
                    }

                    if (ObjectManager.Target.HealthPercentage < 50)
                    {
                        CastSpellIfPossible(divineFavorSpell, true);
                    }

                    if (ObjectManager.Player.ManaPercentage < 50
                       && ObjectManager.Player.ManaPercentage > 20)
                    {
                        CastSpellIfPossible(divineIlluminationSpell, true);
                    }

                    double healthDifference = ObjectManager.Target.MaxHealth - ObjectManager.Target.Health;
                    List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                    foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                    {
                        if (CastSpellIfPossible(keyValuePair.Value, true))
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                if (MyAuraManager.Tick())
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick())
            {
                return;
            }
        }

        private void HandleTargetSelection(List<WowPlayer> possibleTargets)
        {
            // select the one with lowest hp
            WowUnit target = possibleTargets.Where(e => !e.IsDead && e.Health > 1).OrderBy(e => e.HealthPercentage).First();

            if (target != null)
            {
                HookManager.TargetGuid(target.Guid);
            }
        }

        private bool NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing)
        {
            IEnumerable<WowPlayer> players = ObjectManager.WowObjects.OfType<WowPlayer>();
            List<WowPlayer> groupPlayers = players.Where(e => !e.IsDead && e.Health > 1 && ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

            groupPlayers.Add(ObjectManager.Player);

            playersThatNeedHealing = groupPlayers.Where(e => e.HealthPercentage < 90).ToList();

            return playersThatNeedHealing.Count > 0;
        }
    }
}
