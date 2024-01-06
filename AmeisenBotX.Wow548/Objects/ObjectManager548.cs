using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects;

namespace AmeisenBotX.Wow548.Objects
{
    public class ObjectManager548 : ObjectManager<WowObject548, WowUnit548, WowPlayer548, WowGameobject548, WowDynobject548, WowItem548, WowCorpse548, WowContainer548>
    {
        public ObjectManager548(WowMemoryApi memory)
            : base(memory)
        {
        }

        protected override void ReadParty()
        {
            if (ReadPartyPointer(out IntPtr party)
                && Memory.Read(IntPtr.Add(party, 0xC4), out int count) && count > 0)
            {
                PartymemberGuids = ReadPartymemberGuids(party);
                Partymembers = wowObjects.OfType<IWowUnit>().Where(e => PartymemberGuids.Contains(e.Guid));

                Vector3 pos = new();

                foreach (Vector3 vec in Partymembers.Select(e => e.Position))
                {
                    pos += vec;
                }

                CenterPartyPosition = pos / Partymembers.Count();

                PartyPetGuids = PartyPets.Select(e => e.Guid);
                PartyPets = wowObjects.OfType<IWowUnit>().Where(e => PartymemberGuids.Contains(e.SummonedByGuid));
            }
        }

        private IEnumerable<ulong> ReadPartymemberGuids(IntPtr party)
        {
            List<ulong> partymemberGuids = [];

            for (int i = 0; i < 40; i++)
            {
                if (Memory.Read(IntPtr.Add(party, i * 4), out IntPtr player) && player != IntPtr.Zero
                    && Memory.Read(IntPtr.Add(player, 0x10), out ulong guid) && guid > 0)
                {
                    partymemberGuids.Add(guid);

                    if (Memory.Read(IntPtr.Add(player, 0x4), out int status) && status == 2)
                    {
                        PartyleaderGuid = guid;
                    }
                }
            }

            return partymemberGuids.Where(e => e != 0 && e != PlayerGuid).Distinct();
        }

        private bool ReadPartyPointer(out IntPtr party)
        {
            return Memory.Read(Memory.Offsets.PartyLeader, out party) && party != IntPtr.Zero;
        }
    }
}