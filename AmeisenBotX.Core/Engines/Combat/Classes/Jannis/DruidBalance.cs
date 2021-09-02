using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Logic.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class DruidBalance : BasicCombatClass
    {
        public DruidBalance(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.MoonkinForm, () => TryCastSpell(Druid335a.MoonkinForm, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.Thorns, () => TryCastSpell(Druid335a.Thorns, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.MarkOfTheWild, () => TryCastSpell(Druid335a.MarkOfTheWild, Bot.Wow.PlayerGuid, true, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.Moonfire, () => LunarEclipse && TryCastSpell(Druid335a.Moonfire, Bot.Wow.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.InsectSwarm, () => SolarEclipse && TryCastSpell(Druid335a.InsectSwarm, Bot.Wow.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(Druid335a.FaerieFire, x.Guid, true) },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((Druid335a.MarkOfTheWild, (spellName, guid) => TryCastSpell(spellName, guid, true)));

            SolarEclipse = false;
            LunarEclipse = true;
        }

        public override string Description => "FCFS based CombatClass for the Balance (Owl) Druid spec.";

        public override string DisplayName => "Druid Balance";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.Shield }, new() { WowWeaponType.Sword, WowWeaponType.Mace, WowWeaponType.Axe });

        public DateTime LastEclipseCheck { get; private set; }

        public bool LunarEclipse { get; set; }

        public override WowRole Role => WowRole.Dps;

        public bool SolarEclipse { get; set; }

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 5) },
                { 3, new(1, 3, 1) },
                { 4, new(1, 4, 2) },
                { 5, new(1, 5, 2) },
                { 7, new(1, 7, 3) },
                { 8, new(1, 8, 1) },
                { 9, new(1, 9, 2) },
                { 10, new(1, 10, 5) },
                { 11, new(1, 11, 3) },
                { 12, new(1, 12, 3) },
                { 13, new(1, 13, 1) },
                { 16, new(1, 16, 3) },
                { 17, new(1, 17, 2) },
                { 18, new(1, 18, 1) },
                { 19, new(1, 19, 3) },
                { 20, new(1, 20, 3) },
                { 22, new(1, 22, 5) },
                { 23, new(1, 23, 3) },
                { 25, new(1, 25, 1) },
                { 26, new(1, 26, 2) },
                { 27, new(1, 27, 3) },
                { 28, new(1, 28, 1) },
            },
            Tree2 = new(),
            Tree3 = new()
            {
                { 1, new(3, 1, 2) },
                { 3, new(3, 3, 5) },
                { 6, new(3, 6, 3) },
                { 7, new(3, 7, 3) },
                { 8, new(3, 8, 1) },
                { 9, new(3, 9, 2) },
            },
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Druid;

        public override void Execute()
        {
            base.Execute();

            CheckForEclipseProcs();

            if (SelectTarget(TargetProviderDps))
            {
                if (TryCastSpell(Druid335a.NaturesGrasp, 0))
                {
                    return;
                }

                double distance = Bot.Target.Position.GetDistance(Bot.Player.Position);

                if (distance < 12.0
                    && Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Druid335a.EntanglingRoots)
                    && TryCastSpellDk(Druid335a.EntanglingRoots, Bot.Wow.TargetGuid, false, false, true))
                {
                    return;
                }

                if (NeedToHealMySelf())
                {
                    return;
                }

                if ((Bot.Player.ManaPercentage < 30
                        && TryCastSpell(Druid335a.Innervate, 0))
                    || (Bot.Player.HealthPercentage < 70
                        && TryCastSpell(Druid335a.Barkskin, 0, true))
                    || (LunarEclipse
                        && TryCastSpell(Druid335a.Starfire, Bot.Wow.TargetGuid, true))
                    || (SolarEclipse
                        && TryCastSpell(Druid335a.Wrath, Bot.Wow.TargetGuid, true))
                    || (Bot.Objects.WowObjects.OfType<IWowUnit>().Where(e => !e.IsInCombat && Bot.Player.Position.GetDistance(e.Position) < 35).Count() < 4
                        && TryCastSpell(Druid335a.Starfall, Bot.Wow.TargetGuid, true)))
                {
                    return;
                }

                if (TryCastSpell(Druid335a.ForceOfNature, 0, true))
                {
                    Bot.Wow.ClickOnTerrain(Bot.Player.Position);
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (NeedToHealMySelf())
            {
                return;
            }
        }

        private bool CheckForEclipseProcs()
        {
            if (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Druid335a.EclipseLunar))
            {
                SolarEclipse = false;
                LunarEclipse = true;
            }
            else if (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Druid335a.EclipseSolar))
            {
                SolarEclipse = true;
                LunarEclipse = false;
            }

            LastEclipseCheck = DateTime.Now;
            return false;
        }

        private bool NeedToHealMySelf()
        {
            if (Bot.Player.HealthPercentage < 60
                && !Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Druid335a.Rejuvenation)
                && TryCastSpell(Druid335a.Rejuvenation, 0, true))
            {
                return true;
            }

            if (Bot.Player.HealthPercentage < 40
                && TryCastSpell(Druid335a.HealingTouch, 0, true))
            {
                return true;
            }

            return false;
        }
    }
}