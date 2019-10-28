using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class WarriorArms : ICombatClass
    {
        public WarriorArms(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
        }

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => false;

        private CharacterManager CharacterManager { get; }

        private DateTime HeroicStrikeLastUsed { get; set; }

        private HookManager HookManager { get; }

        private Vector3 LastPosition { get; set; }

        private ObjectManager ObjectManager { get; }

        public void Execute()
        {
            ulong targetGuid = ObjectManager.TargetGuid;
            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == targetGuid);
            if (target != null)
            {
                // make sure we're auto attacking
                if (!ObjectManager.Player.IsAutoAttacking)
                {
                    HookManager.StartAutoAttack();
                }

                HandleMovement(target);
                HandleAttacking(target);
            }
        }

        private void HandleAttacking(WowUnit target)
        {
            double playerRage = ObjectManager.Player.Rage;
            double distanceToTarget = ObjectManager.Player.Position.GetDistance(target.Position);
            double targetHealthPercent = (target.Health / (double)target.MaxHealth) * 100;
            double playerHealthPercent = (ObjectManager.Player.Health / (double)ObjectManager.Player.MaxHealth) * 100.0;
            (string, int) targetCastingInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);

            if (HookManager.GetSpellCooldown("Enraged Regeneration") <= 0
                && playerRage >= 15 && playerHealthPercent <= 50)
            {
                HookManager.CastSpell("Enraged Regeneration");
            }

            if (HookManager.GetSpellCooldown("Retaliation") <= 0
                && playerHealthPercent <= 40)
            {
                HookManager.CastSpell("Retaliation");
            }

            if (HookManager.GetSpellCooldown("Intimidating Shout") <= 0
                && playerRage >= 25 && playerHealthPercent <= 50)
            {
                HookManager.CastSpell("Intimidating Shout");
            }

            if (HookManager.GetSpellCooldown("Intimidating Shout") <= 0
                && playerRage >= 25 && targetCastingInfo.Item2 > 1)
            {
                HookManager.CastSpell("Intimidating Shout");
            }

            if (HookManager.GetSpellCooldown("Bloodrage") <= 0
                && playerRage <= 20 && playerHealthPercent >= 15)
            {
                HookManager.CastSpell("Bloodrage");
            }

            if (HookManager.GetSpellCooldown("Charge") <= 0
                && distanceToTarget > 8 && distanceToTarget < 25)
            {
                HookManager.CastSpell("Charge");
            }

            if (HookManager.GetSpellCooldown("Intercept") <= 0
                && distanceToTarget > 8 && distanceToTarget < 25 && playerRage >= 10)
            {
                HookManager.CastSpell("Intercept");
            }

            if (HookManager.GetSpellCooldown("Bladestorm") <= 0
                && playerRage >= 25)
            {
                HookManager.CastSpell("Bladestorm");
            }

            if (HookManager.GetSpellCooldown("Mortal Strike") <= 0
                && playerRage >= 30 && distanceToTarget < 3)
            {
                HookManager.CastSpell("Mortal Strike");
            }

            if (HeroicStrikeLastUsed + TimeSpan.FromSeconds(6) < DateTime.Now
                && playerRage >= 12 && distanceToTarget < 3)
            {
                HookManager.CastSpell("Heroic Strike");
                HeroicStrikeLastUsed = DateTime.Now;
            }

            if (HeroicStrikeLastUsed + TimeSpan.FromSeconds(6) < DateTime.Now
                && playerRage >= 15 && distanceToTarget < 3)
            {
                HookManager.CastSpell("Slam");
                HeroicStrikeLastUsed = DateTime.Now;
            }

            if (HeroicStrikeLastUsed + TimeSpan.FromSeconds(30) < DateTime.Now
                && playerRage >= 10 && distanceToTarget < 3)
            {
                HookManager.CastSpell("Rend");
                HeroicStrikeLastUsed = DateTime.Now;
            }

            if (target.IsFleeing && distanceToTarget < 3 && playerRage >= 12)
            {
                HookManager.CastSpell("Hamstring");
            }

            if (playerRage >= 15 && distanceToTarget < 3 && targetHealthPercent <= 20)
            {
                HookManager.CastSpell("Execute");
            }

            if (HookManager.GetSpellCooldown("Berserker Rage") <= 0
                && (ObjectManager.Player.IsConfused || ObjectManager.Player.IsDazed || ObjectManager.Player.IsSilenced || ObjectManager.Player.IsFleeing))
            {
                HookManager.CastSpell("Berserker Rage");
            }

            if (HookManager.GetSpellCooldown("Heroic Throw") <= 0
                && distanceToTarget <= 30 && distanceToTarget >= 10)
            {
                HookManager.CastSpell("Heroic Throw");
            }

            if (HookManager.GetSpellCooldown("Shattering Throw") <= 0
                && distanceToTarget <= 30 && distanceToTarget >= 10 && playerRage >= 25)
            {
                HookManager.CastSpell("Shattering Throw");
            }
        }

        private void HandleMovement(WowUnit target)
        {
            double distanceTraveled = ObjectManager.Player.Position.GetDistance(LastPosition);
            CharacterManager.MoveToPosition(target.Position);

            if (distanceTraveled < 0.001 && distanceTraveled > 0)
            {
                CharacterManager.Jump();
            }

            LastPosition = ObjectManager.Player.Position;
        }
    }
}