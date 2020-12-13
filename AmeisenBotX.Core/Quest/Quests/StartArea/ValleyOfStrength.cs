using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Quest.Quests.StartArea
{
    class ValleyOfStrength
    {
        public BotQuest QCuttingTeeth;
        
        public ValleyOfStrength(WowInterface wowInterface)
        {
            QCuttingTeeth = new BotQuest(
                wowInterface, 788, "Cutting Teeth", 55, 1,
                () => (wowInterface.ObjectManager.GetClosestWowUnitByDisplayId(new List<int> { 1653 }), new Vector3(-600, -4186, 41)),
                () => (wowInterface.ObjectManager.GetClosestWowUnitByDisplayId(new List<int> { 1653 }), new Vector3(-600, -4186, 41)),
                new List<IQuestObjective>()
                {
                    new QuestObjectiveChain(new List<IQuestObjective>()
                    {
                        new MoveToPositionQuestObjective(wowInterface, new Vector3(-375, -4314, 51), 80.0),
                        new KillUnitQuestObjective(wowInterface, new Dictionary<int, int> { { 0, 503 } }, () => wowInterface.ObjectManager.Player.QuestlogEntries.FirstOrDefault(e => e.Id == 788).Finished == 1)
                    })
                });
        }
    }
}
