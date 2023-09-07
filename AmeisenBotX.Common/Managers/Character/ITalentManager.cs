using AmeisenBotX.Core.Managers.Character.Talents.Objects;

namespace AmeisenBotX.Core.Managers.Character.Talents
{
    public interface ITalentManager
    {
        ITalentTree TalentTree { get; set; }

        void SelectTalents(ITalentTree wantedTalents, int talentPoints);
        void Update();
    }
}