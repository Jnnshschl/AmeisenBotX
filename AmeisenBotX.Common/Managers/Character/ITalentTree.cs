using System.Collections.Generic;

namespace AmeisenBotX.Core.Managers.Character.Talents.Objects
{
    public interface ITalentTree
    {
        Dictionary<int, Talent> Tree1 { get; set; }
        Dictionary<int, Talent> Tree2 { get; set; }
        Dictionary<int, Talent> Tree3 { get; set; }

        Dictionary<int, Dictionary<int, Talent>> AsDict();
    }
}