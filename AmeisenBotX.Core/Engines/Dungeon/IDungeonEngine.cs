﻿using AmeisenBotX.Core.Engines.Dungeon.Objects;
using AmeisenBotX.Core.Engines.Jobs.Profiles;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Dungeon
{
    public interface IDungeonEngine
    {
        /// <summary>
        /// Get the currently loaded dungeon path.
        /// </summary>
        List<DungeonNode> Nodes { get; }

        /// <summary>
        /// Get current loaded dungeon profile.
        /// </summary>
        IDungeonProfile Profile { get; }

        /// <summary>
        /// Called once when we enter the dungeon state.
        /// </summary>
        void Enter();

        /// <summary>
        /// Polled as long as the player is in the dungeon.
        /// </summary>
        void Execute();

        /// <summary>
        /// Called once when we exit the dungeon state.
        /// </summary>
        void Exit();

        /// <summary>
        /// Use this to notify the dungeon engine that the player is dead.
        /// </summary>
        void OnDeath();

        /// <summary>
        /// Try to get a dungeon profile for the given map id, return null if bo profile was found.
        /// </summary>
        IDungeonProfile TryGetProfileByMapId(WowMapId mapId);
    }
}