using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Battleground.KamelBG.Enums;
using System;
using System.Linq;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Common;
using System.Collections.Generic;
using AmeisenBotX.Logging;

namespace AmeisenBotX.Core.Battleground.KamelBG
{
    internal class StrandOfTheAncients : IBattlegroundEngine
    {
        public string Author => "Lukas";

        public string Description => "Strand of the Ancients";

        public string Name => "Strand of the Ancients";

        public WowInterface WowInterface { get; }

        public StrandOfTheAncients(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
        }

        public void Enter()
        {
            throw new NotImplementedException();
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }

        public void Leave()
        {
            throw new NotImplementedException();
        }
    }
}
