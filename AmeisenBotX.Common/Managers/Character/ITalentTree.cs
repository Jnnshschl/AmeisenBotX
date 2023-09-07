using System.Collections.Generic;

namespace AmeisenBotX.Core.Managers.Character.Talents.Objects
{
    public interface ITalentTree
    {
        Dictionary<int, ITalent> Tree1 { get; set; }
        Dictionary<int, ITalent> Tree2 { get; set; }
        Dictionary<int, ITalent> Tree3 { get; set; }

        Dictionary<int, Dictionary<int, ITalent>> AsDict();
    }
}