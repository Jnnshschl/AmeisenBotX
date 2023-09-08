using AmeisenBotX.Core.Managers.Character.Spells.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Managers.Character.Spells
{
    public interface ISpellBook
    {
        IEnumerable<Spell> Spells { get; }

        Spell GetSpellByName(string spellname);
        bool IsSpellKnown(string spellname);
        bool TryGetSpellByName(string spellname, out Spell spell);
        void Update();

        delegate void SpellBookUpdate();

        event SpellBookUpdate OnSpellBookUpdate;
    }
}