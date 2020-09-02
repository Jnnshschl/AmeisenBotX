using AmeisenBotX.Core.Data.Objects.Enums;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WowAura
    {
        public ulong Creator;
        public int SpellId;
        public byte Flags;
        public byte Level;
        public ushort StackCount;
        public uint Duration;
        public uint EndTime;

        public bool IsActive => ((AuraFlags)Flags).HasFlag(AuraFlags.Active);

        public bool IsHarmful => ((AuraFlags)Flags).HasFlag(AuraFlags.Harmful);

        public bool IsPassive => ((AuraFlags)Flags).HasFlag(AuraFlags.Passive);

        public string Name
        {
            get
            {
                if (!WowInterface.I.BotCache.TryGetSpellName(SpellId, out string name))
                {
                    name = WowInterface.I.HookManager.GetSpellNameById(SpellId);
                    WowInterface.I.BotCache.CacheSpellName(SpellId, name);
                }

                return name;
            }
        }

        public override string ToString()
        {
            return $"{SpellId} (lvl. {Level}) x{StackCount} [CG: {Creator}], Harmful: {IsHarmful}, Passive: {IsPassive}";
        }
    }
}