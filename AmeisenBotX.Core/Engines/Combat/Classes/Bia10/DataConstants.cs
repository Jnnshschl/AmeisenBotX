using System;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Bia10
{
    public static class DataConstants
    {
        public struct ShamanSpells
        {
            public const string AncestralSpirit = "Ancestral Spirit";
            public const string ChainHeal = "Chain Heal";
            public const string ChainLightning = "Chain Lightning";
            public const string EarthlivingBuff = "Earthliving ";
            public const string EarthlivingWeapon = "Earthliving Weapon";
            public const string EarthShield = "Earth Shield";
            public const string EarthShock = "Earth Shock";
            public const string ElementalMastery = "Elemental Mastery";
            public const string FeralSpirit = "Feral Spirit";
            public const string FlameShock = "Flame Shock";
            public const string FlametongueBuff = "Flametongue";
            public const string FlametongueWeapon = "Flametongue Weapon";
            public const string HealingWave = "Healing Wave";
            public const string Heroism = "Heroism";
            public const string Hex = "Hex";
            public const string LavaBurst = "Lava Burst";
            public const string LavaLash = "Lava Lash";
            public const string LesserHealingWave = "Lesser Healing Wave";
            public const string LightningBolt = "Lightning Bolt";
            public const string LightningShield = "Lightning Shield";
            public const string MaelstromWeapon = "Mealstrom Weapon";
            public const string Riptide = "Riptide";
            public const string ShamanisticRage = "Shamanistic Rage";
            public const string Stormstrike = "Stormstrike";
            public const string Thunderstorm = "Thunderstorm";
            public const string TidalForce = "Tidal Force";
            public const string WaterShield = "Water Shield";
            public const string WindfuryBuff = "Windfury";
            public const string WindfuryWeapon = "Windfury Weapon";
            public const string WindShear = "Wind Shear";
        }

        public struct Racials
        {
            public const string Berserking = "Berserking"; // Troll
            public const string BloodFury = "Blood Fury";  // Orc
        }

        public const float MAX_ANGLE = MathF.PI * 2.0f;

        public static readonly int[] usableHealingItems = {
            // food
            117,
            // potions
            118, 929, 1710, 2938, 3928, 4596, 5509, 13446, 22829, 33447,
            // healthstones
            5509, 5510, 5511, 5512, 9421, 19013, 22103, 36889, 36892,
        };

        public static readonly int[] usableManaItems = {
            // drinks
            159,
            // potions
            2245, 3385, 3827, 6149, 13443, 13444, 33448, 22832,
        };

        public static string GetCastSpellString(string spellName, bool castOnSelf)
        {
            return
                $"{{v:3}},{{v:4}}=GetSpellCooldown(\"{spellName}\"){{v:2}}=({{v:3}}+{{v:4}}-GetTime())*1000;if {{v:2}}<=0 then {{v:2}}=0;CastSpellByName(\"{spellName}\"{(castOnSelf ? ", \"player\"" : string.Empty)}){{v:5}},{{v:6}}=GetSpellCooldown(\"{spellName}\"){{v:1}}=({{v:5}}+{{v:6}}-GetTime())*1000;{{v:0}}=\"1;\"..{{v:1}} else {{v:0}}=\"0;\"..{{v:2}} end";
        }
    }
}