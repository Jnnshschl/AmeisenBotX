using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System.Collections.Generic;
using static AmeisenBotX.Core.Utils.InterruptManager;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class DeathknightFrost : BasicCombatClass
    {
        public DeathknightFrost(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(frostPresenceSpell, () => TryCastSpellDk(frostPresenceSpell, 0)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(hornOfWinterSpell, () => TryCastSpellDk(hornOfWinterSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(frostFeverSpell, () => TryCastSpellDk(icyTouchSpell, WowInterface.ObjectManager.TargetGuid, false, false, false, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bloodPlagueSpell, () => TryCastSpellDk(plagueStrikeSpell, WowInterface.ObjectManager.TargetGuid, false, false, false, true)));

            InterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => TryCastSpellDk(mindFreezeSpell, x.Guid, true) },
                { 1, (x) => TryCastSpellDk(strangulateSpell, x.Guid, false, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Frost Deathknight spec.";

        public override string Displayname => "Deathknight Frost";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<WowArmorType>() { WowArmorType.SHIELDS });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(1, 2, 3) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 3) },
                { 2, new Talent(2, 2, 2) },
                { 5, new Talent(2, 5, 2) },
                { 6, new Talent(2, 6, 3) },
                { 7, new Talent(2, 7, 5) },
                { 9, new Talent(2, 9, 3) },
                { 10, new Talent(2, 10, 5) },
                { 11, new Talent(2, 11, 2) },
                { 12, new Talent(2, 12, 2) },
                { 14, new Talent(2, 14, 3) },
                { 16, new Talent(2, 16, 1) },
                { 17, new Talent(2, 17, 2) },
                { 18, new Talent(2, 18, 3) },
                { 22, new Talent(2, 22, 3) },
                { 23, new Talent(2, 23, 3) },
                { 24, new Talent(2, 24, 1) },
                { 26, new Talent(2, 26, 1) },
                { 27, new Talent(2, 27, 3) },
                { 28, new Talent(2, 28, 5) },
                { 29, new Talent(2, 29, 1) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 2) },
                { 2, new Talent(3, 2, 3) },
                { 4, new Talent(3, 4, 2) },
                { 7, new Talent(3, 7, 3) },
                { 9, new Talent(3, 9, 5) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Deathknight;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetManagerDps))
            {
                if (WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid
                   && TryCastSpellDk(darkCommandSpell, WowInterface.ObjectManager.TargetGuid))
                {
                    return;
                }

                if (!WowInterface.ObjectManager.Target.HasBuffByName(chainsOfIceSpell)
                    && WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 2.0
                    && TryCastSpellDk(chainsOfIceSpell, WowInterface.ObjectManager.TargetGuid, false, false, true))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Target.HasBuffByName(chainsOfIceSpell)
                    && TryCastSpellDk(chainsOfIceSpell, WowInterface.ObjectManager.TargetGuid, false, false, true))
                {
                    return;
                }

                if (TryCastSpellDk(empowerRuneWeapon, 0))
                {
                    return;
                }

                if ((WowInterface.ObjectManager.Player.HealthPercentage < 60
                        && TryCastSpellDk(iceboundFortitudeSpell, 0, true))
                    || TryCastSpellDk(unbreakableArmorSpell, 0, false, false, true)
                    || TryCastSpellDk(obliterateSpell, WowInterface.ObjectManager.TargetGuid, false, false, true, true)
                    || TryCastSpellDk(bloodStrikeSpell, WowInterface.ObjectManager.TargetGuid, false, true)
                    || TryCastSpellDk(deathCoilSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || (WowInterface.ObjectManager.Player.Runeenergy > 60
                        && TryCastSpellDk(runeStrikeSpell, WowInterface.ObjectManager.TargetGuid)))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();
        }
    }
}