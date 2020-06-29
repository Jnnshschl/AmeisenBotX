using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Kamel
{
    class RestorationShaman : BasicKamelClass
    {

        private const string healingWaveSpell = "Healing Wave";

        Dictionary<string, DateTime> spellCoolDown = new Dictionary<string, DateTime>();

        public RestorationShaman(WowInterface wowInterface) : base()
        {
            WowInterface = wowInterface;
            spellCoolDown.Add(healingWaveSpell, DateTime.Now);

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, Utils.AuraManager.CastFunction>();
            WowInterface.CharacterManager.SpellBook.OnSpellBookUpdate += () =>
            {
                //if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(berserkerStanceSpell))
                //{
                //    MyAuraManager.BuffsToKeepActive.Add(berserkerStanceSpell, () => { WowInterface.HookManager.CastSpell(berserkerStanceSpell); return true; });
                //}
                //else if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(battleStanceSpell))
                //{
                //    MyAuraManager.BuffsToKeepActive.Add(battleStanceSpell, () => { WowInterface.HookManager.CastSpell(berserkerStanceSpell); return true; });
                //}
            };
        }

        public override string Author => "Kamel";

        public override bool WalkBehindEnemy => false;

        public override WowClass Class => WowClass.Shaman;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Basic Resto Shaman";

        public override string Displayname => "Shaman Restoration";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override bool UseAutoAttacks => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override string Version => "1.0";

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>(),
            Tree2 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(2, 3, 5) },
                { 5, new Talent(2, 5, 5) },
                { 7, new Talent(2, 7, 3) },
                { 8, new Talent(2, 8, 1) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 5) },
                { 5, new Talent(3, 5, 5) },
                { 6, new Talent(3, 6, 3) },
                { 7, new Talent(3, 7, 3) },
                { 8, new Talent(3, 8, 1) },
                { 9, new Talent(3, 9, 3) },
                { 10, new Talent(3, 10, 3) },
                { 11, new Talent(3, 11, 5) },
                { 12, new Talent(3, 12, 3) },
                { 13, new Talent(3, 13, 1) },
                { 15, new Talent(3, 15, 5) },
                { 17, new Talent(3, 17, 1) },
                { 19, new Talent(3, 19, 2) },
                { 20, new Talent(3, 20, 2) },
                { 21, new Talent(3, 21, 3) },
                { 22, new Talent(3, 22, 3) },
                { 23, new Talent(3, 23, 1) },
                { 24, new Talent(3, 24, 2) },
                { 25, new Talent(3, 25, 5) },
                { 26, new Talent(3, 26, 1) },
            },
        };

        public override void ExecuteCC()
        {
            if (WowInterface.ObjectManager.TargetGuid != 0)
            {
                if (healingWaveSpellEvent.Run() && WowInterface.ObjectManager.Target.HealthPercentage < 100)
                {
                    WowInterface.HookManager.CastSpell(healingWaveSpell);
                    spellCoolDown[healingWaveSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(healingWaveSpell));
                    return;
                }
            }
            else if (TargetSelectEvent.Run())
            {
                //WowUnit partyMember = WowInterface.ObjectManager.GetNearPartymembers(WowInterface.ObjectManager.Player.Position, 20).FirstOrDefault();
                WowUnit partyMember = WowInterface.ObjectManager.Partymembers.OrderBy(e => e.HealthPercentage).FirstOrDefault();

                if (partyMember != null)
                {
                    WowInterface.HookManager.TargetGuid(partyMember.Guid);
                    // AmeisenLogger.Instance.Log("FuryWarri", $"Target: {nearTarget}");
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            throw new NotImplementedException();
        }
    }
}
