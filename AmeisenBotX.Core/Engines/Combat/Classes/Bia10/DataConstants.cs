using System;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Bia10
{
    public static class DataConstants
    {
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