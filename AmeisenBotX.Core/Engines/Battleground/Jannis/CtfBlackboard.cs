﻿using AmeisenBotX.BehaviorTree.Interfaces;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis
{
    public class CtfBlackboard : IBlackboard
    {
        public CtfBlackboard(Action updateAction)
        {
            UpdateAction = updateAction;
        }

        public IWowUnit EnemyTeamFlagCarrier { get; set; }

        public Vector3 EnemyTeamFlagPos { get; set; }

        public bool EnemyTeamHasFlag { get; set; }

        public int EnemyTeamMaxScore { get; set; }

        public int EnemyTeamScore { get; set; }

        public IWowUnit MyTeamFlagCarrier { get; set; }

        public Vector3 MyTeamFlagPos { get; set; }

        public bool MyTeamHasFlag { get; set; }

        public int MyTeamMaxScore { get; set; }

        public int MyTeamScore { get; set; }

        public List<IWowGameobject> NearFlags { get; set; }

        private Action UpdateAction { get; }

        public void Update()
        {
            UpdateAction();
        }
    }
}