using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class DeathknightUnholy : BasicCombatClass
    {
        public DeathknightUnholy(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(unholyPresenceSpell, () => TryCastSpellDk(unholyPresenceSpell, 0)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(hornOfWinterSpell, () => TryCastSpellDk(hornOfWinterSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(frostFeverSpell, () => TryCastSpellDk(icyTouchSpell, WowInterface.TargetGuid, false, false, false, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bloodPlagueSpell, () => TryCastSpellDk(plagueStrikeSpell, WowInterface.TargetGuid, false, false, false, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpellDk(mindFreezeSpell, x.Guid, true) },
                { 1, (x) => TryCastSpellDk(strangulateSpell, x.Guid, false, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Unholy Deathknight spec.";

        public override string Displayname => "Deathknight Unholy";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new() { WowArmorType.SHIELDS });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 2) },
                { 2, new(1, 2, 3) },
                { 4, new(1, 4, 5) },
                { 6, new(1, 6, 2) },
                { 8, new(1, 8, 5) },
            },
            Tree2 = new(),
            Tree3 = new()
            {
                { 1, new(3, 1, 2) },
                { 2, new(3, 2, 3) },
                { 4, new(3, 4, 2) },
                { 7, new(3, 7, 3) },
                { 8, new(3, 8, 3) },
                { 9, new(3, 9, 5) },
                { 12, new(3, 12, 3) },
                { 13, new(3, 13, 2) },
                { 14, new(3, 14, 1) },
                { 15, new(3, 15, 5) },
                { 16, new(3, 16, 2) },
                { 20, new(3, 20, 1) },
                { 21, new(3, 21, 5) },
                { 25, new(3, 25, 3) },
                { 26, new(3, 26, 1) },
                { 27, new(3, 27, 3) },
                { 28, new(3, 28, 3) },
                { 29, new(3, 29, 1) },
                { 30, new(3, 30, 5) },
                { 31, new(3, 31, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Deathknight;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if (WowInterface.Target.TargetGuid != WowInterface.PlayerGuid
                   && TryCastSpellDk(darkCommandSpell, WowInterface.TargetGuid))
                {
                    return;
                }

                if (!WowInterface.Target.HasBuffByName(chainsOfIceSpell)
                    && WowInterface.Target.Position.GetDistance(WowInterface.Player.Position) > 2.0
                    && TryCastSpellDk(chainsOfIceSpell, WowInterface.TargetGuid, false, false, true))
                {
                    return;
                }

                if (WowInterface.Target.HasBuffByName(chainsOfIceSpell)
                    && TryCastSpellDk(chainsOfIceSpell, WowInterface.TargetGuid, false, false, true))
                {
                    return;
                }

                if (TryCastSpellDk(empowerRuneWeapon, 0))
                {
                    return;
                }

                if ((WowInterface.Player.HealthPercentage < 60
                        && TryCastSpellDk(iceboundFortitudeSpell, WowInterface.TargetGuid, true))
                    || TryCastSpellDk(bloodStrikeSpell, WowInterface.TargetGuid, false, true)
                    || TryCastSpellDk(scourgeStrikeSpell, WowInterface.TargetGuid, false, false, true, true)
                    || TryCastSpellDk(deathCoilSpell, WowInterface.TargetGuid, true)
                    || TryCastSpellDk(summonGargoyleSpell, WowInterface.TargetGuid, true)
                    || (WowInterface.Player.Runeenergy > 60
                        && TryCastSpellDk(runeStrikeSpell, WowInterface.TargetGuid)))
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