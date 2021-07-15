using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Learning.Sessions.Combat
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SpellUsageCombatSessionDataset
    {
        public DateTime Timestamp { get; set; }

        public bool IsPlayer { get; set; }

        public bool EnemyIsPlayer { get; set; }

        public ulong Id { get; set; }

        public ulong EnemyId { get; set; }

        public bool IsDamage { get; set; }

        public int WowRace { get; set; }

        public int WowClass { get; set; }

        public int WowPowertype { get; set; }

        public int EnemyWowRace { get; set; }

        public int EnemyWowClass { get; set; }

        public int EnemyWowPowertype { get; set; }

        public int Health { get; set; }

        public int EnemyHealth { get; set; }

        public int Energy { get; set; }

        public int EnemyEnergy { get; set; }

        public int MaxHealth { get; set; }

        public int MaxEnemyHealth { get; set; }

        public int MaxEnergy { get; set; }

        public int MaxEnemyEnergy { get; set; }

        public int Level { get; set; }

        public int EnemyLevel { get; set; }

        public int SpellId { get; set; }

        public int Impact { get; set; }

        public int OverImpact { get; set; }
    }

    /// <summary>
    /// Learning session to decide which damage/heal spells to use.
    /// </summary>
    public class SpellUsageCombatSession
    {
        public List<SpellUsageCombatSessionDataset> Datasets { get; set; }

        [JsonIgnore()]
        public DateTime EndTime => Datasets.LastOrDefault().Timestamp;

        [JsonIgnore()]
        public DateTime StartTime => Datasets.FirstOrDefault().Timestamp;

        public SpellUsageCombatSession()
        {
            Datasets = new();
        }

        public void AddData
        (
            bool isPlayer,
            bool enemyIsPlayer,
            ulong id,
            ulong enemyId,
            bool isDamage,
            int wowRace,
            int wowClass,
            int wowPowertype,
            int enemyWowRace,
            int enemyWowClass,
            int enemyWowPowertype,
            int health,
            int enemyHealth,
            int energy,
            int enemyEnergy,
            int maxHealth,
            int maxEnemyHealth,
            int maxEnergy,
            int maxEnemyEnergy,
            int level,
            int enemyLevel,
            int spellId,
            int impact,
            int overImpact
        )
        {
            Datasets.Add
            (
                new SpellUsageCombatSessionDataset
                {
                    Timestamp = DateTime.Now,
                    IsPlayer = isPlayer,
                    EnemyIsPlayer = enemyIsPlayer,
                    Id = id,
                    EnemyId = enemyId,
                    WowRace = wowRace,
                    WowClass = wowClass,
                    WowPowertype = wowPowertype,
                    EnemyWowRace = enemyWowRace,
                    EnemyWowClass = enemyWowClass,
                    EnemyWowPowertype = enemyWowPowertype,
                    IsDamage = isDamage,
                    Health = health,
                    EnemyHealth = enemyHealth,
                    Energy = energy,
                    EnemyEnergy = enemyEnergy,
                    MaxHealth = maxHealth,
                    MaxEnemyHealth = maxEnemyHealth,
                    MaxEnergy = maxEnergy,
                    MaxEnemyEnergy = maxEnemyEnergy,
                    Level = level,
                    EnemyLevel = enemyLevel,
                    SpellId = spellId,
                    Impact = impact,
                    OverImpact = overImpact
                }
            );
        }
    }
}