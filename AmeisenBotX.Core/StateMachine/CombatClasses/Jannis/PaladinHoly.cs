using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
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

        public PaladinHoly(WowInterface wowInterface) : base(wowInterface)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { blessingOfWisdomSpell, () => CastSpellIfPossible(blessingOfWisdomSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { devotionAuraSpell, () => CastSpellIfPossible(devotionAuraSpell, WowInterface.ObjectManager.PlayerGuid, true) }
            };

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, flashOfLightSpell },
                { 2000, holyShockSpell },
                { 10000, holyLightSpell }
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Paladin;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Holy Paladin spec.";

        public override string Displayname => "Paladin Holy";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => true;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicSpiritComparator();

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override string Version => "1.0";

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.ManaPercentage < 80
                && CastSpellIfPossible(divinePleaSpell, WowInterface.ObjectManager.PlayerGuid, true))
            {
                return;
            }

            if (NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing))
            {
                HandleTargetSelection(playersThatNeedHealing);
                WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Player);

                if (WowInterface.ObjectManager.Target != null)
                {
                    WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Target);

                    if (WowInterface.ObjectManager.Target.HealthPercentage < 12
                        && CastSpellIfPossible(layOnHandsSpell, 0))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Target.HealthPercentage < 50)
                    {
                        CastSpellIfPossible(divineFavorSpell, WowInterface.ObjectManager.TargetGuid, true);
                    }

                    if (WowInterface.ObjectManager.Player.ManaPercentage < 50
                       && WowInterface.ObjectManager.Player.ManaPercentage > 20)
                    {
                        CastSpellIfPossible(divineIlluminationSpell, 0, true);
                    }

                    double healthDifference = WowInterface.ObjectManager.Target.MaxHealth - WowInterface.ObjectManager.Target.Health;
                    List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                    foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                    {
                        if (CastSpellIfPossible(keyValuePair.Value, WowInterface.ObjectManager.TargetGuid, true))
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

                WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Player);
                WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Target);

                // basic auto attack defending
                if (WowInterface.ObjectManager.TargetGuid == 0 || WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Target) == WowUnitReaction.Friendly)
                {
                    IEnumerable<WowUnit> nearEnemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 10).Where(e => e.IsInCombat);

                    if (nearEnemies.Count() > 0)
                    {
                        WowUnit target = nearEnemies.FirstOrDefault();
                        if (target != null)
                        {
                            WowInterface.HookManager.TargetGuid(target.Guid);
                            WowInterface.MovementEngine.Reset();
                        }
                    }
                }
                else
                {
                    if (WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 4)
                    {
                        WowInterface.MovementEngine.SetState(MovementEngineState.Moving, WowInterface.ObjectManager.Target.Position);
                        WowInterface.MovementEngine.Execute();
                    }
                    else
                    {
                        WowInterface.HookManager.StartAutoAttack();

                        if (!BotMath.IsFacing(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, WowInterface.ObjectManager.Target.Position))
                        {
                            WowInterface.HookManager.FacePosition(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Target.Position);
                        }
                    }
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
                WowInterface.HookManager.TargetGuid(target.Guid);
            }
        }

        private bool NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing)
        {
            IEnumerable<WowPlayer> players = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>();
            List<WowPlayer> groupPlayers = players.Where(e => !e.IsDead && e.Health > 1 && WowInterface.ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

            groupPlayers.Add(WowInterface.ObjectManager.Player);

            playersThatNeedHealing = groupPlayers.Where(e => e.HealthPercentage < 90).ToList();

            return playersThatNeedHealing.Count > 0;
        }
    }
}