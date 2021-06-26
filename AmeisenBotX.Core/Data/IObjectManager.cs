using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Data.Objects.Raw;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data
{
    public interface IObjectManager
    {
        /// <summary>
        /// Fires everytime an object update is done.
        /// </summary>
        event Action<IEnumerable<WowObject>> OnObjectUpdateComplete;

        /// <summary>
        /// Contains information about the current camera.
        /// </summary>
        RawCameraInfo Camera { get; }

        /// <summary>
        /// Contains the current game state when not ingame.
        /// </summary>
        string GameState { get; }

        /// <summary>
        /// Returns whether the current target is in line of sight or not.
        /// </summary>
        bool IsTargetInLineOfSight { get; }

        /// <summary>
        /// Returns true when the world is loaded and the player is ingame.
        /// </summary>
        bool IsWorldLoaded { get; }

        /// <summary>
        /// Last selected target.
        /// </summary>
        WowUnit LastTarget { get; }

        /// <summary>
        /// Last selected target guid.
        /// </summary>
        ulong LastTargetGuid { get; }

        /// <summary>
        /// The current map id the player is on.
        /// </summary>
        WowMapId MapId { get; }

        /// <summary>
        /// The average of the groups players position.
        /// </summary>
        Vector3 MeanGroupPosition { get; }

        /// <summary>
        /// Amount of objects in the wow object manager.
        /// </summary>
        int ObjectCount { get; }

        /// <summary>
        /// The current partyleader.
        /// </summary>
        WowUnit Partyleader { get; }

        /// <summary>
        /// The current paryleaders guid.
        /// </summary>
        ulong PartyleaderGuid { get; }

        /// <summary>
        /// List of all partymembers guids.
        /// </summary>
        IEnumerable<ulong> PartymemberGuids { get; }

        /// <summary>
        /// List of all partymembers.
        /// </summary>
        IEnumerable<WowUnit> Partymembers { get; }

        /// <summary>
        /// List of all party pets guids.
        /// </summary>
        IEnumerable<ulong> PartyPetGuids { get; }

        /// <summary>
        /// List of all party pets.
        /// </summary>
        IEnumerable<WowUnit> PartyPets { get; }

        /// <summary>
        /// The current pet.
        /// </summary>
        WowUnit Pet { get; }

        /// <summary>
        /// The current pets guid.
        /// </summary>
        ulong PetGuid { get; }

        /// <summary>
        /// The current local player.
        /// </summary>
        WowPlayer Player { get; }

        /// <summary>
        /// The current local players guid.
        /// </summary>
        ulong PlayerGuid { get; }

        /// <summary>
        /// The current target.
        /// </summary>
        WowUnit Target { get; }

        /// <summary>
        /// The current tragets guid.
        /// </summary>
        ulong TargetGuid { get; }

        /// <summary>
        /// The current vehicle the local player is in.
        /// </summary>
        WowUnit Vehicle { get; }

        /// <summary>
        /// List of all wow objects.
        /// </summary>
        IEnumerable<WowObject> WowObjects { get; }

        /// <summary>
        /// The current zone id.
        /// </summary>
        int ZoneId { get; }

        /// <summary>
        /// The current zone name.
        /// </summary>
        string ZoneName { get; }

        /// <summary>
        /// The current sub zone name.
        /// </summary>
        string ZoneSubName { get; }

        /// <summary>
        /// Returns a list of all aoe spells in the given area.
        /// </summary>
        /// <param name="position">Center of the area</param>
        /// <param name="onlyEnemy">Get only hostile spells</param>
        /// <param name="extends">Radius of the are</param>
        /// <returns>All matched aoe spells</returns>
        IEnumerable<WowDynobject> GetAoeSpells(Vector3 position, bool onlyEnemy = true, float extends = 1.0f);

        /// <summary>
        /// Returns the closest gameobject that matches the given on of display ids.
        /// </summary>
        /// <param name="displayIds">Display ids to match</param>
        /// <returns>Matched gameobjects</returns>
        WowGameobject GetClosestWowGameobjectByDisplayId(IEnumerable<int> displayIds);

        /// <summary>
        /// Returns the closest unit that matches the given on of display ids.
        /// </summary>
        /// <param name="displayIds">Display ids to match</param>
        /// <param name="onlyQuestgiver">Match only questgivers</param>
        /// <returns>Matched units</returns>
        WowUnit GetClosestWowUnitByDisplayId(IEnumerable<int> displayIds, bool onlyQuestgiver = true);

        /// <summary>
        /// Returns the closest unit that matches the given on of npc ids.
        /// </summary>
        /// <param name="npcIds">Npc ids to match</param>
        /// <param name="onlyQuestgiver">Match only questgivers</param>
        /// <returns>Matched units</returns>
        WowUnit GetClosestWowUnitByNpcId(IEnumerable<int> npcIds, bool onlyQuestgiver = true);

        /// <summary>
        /// Get all enemies that attack a partymember in a certain area.
        /// </summary>
        /// <typeparam name="T">WowUnit or WowPlayer</typeparam>
        /// <param name="position">Center of the area</param>
        /// <param name="distance">Radius of the area</param>
        /// <returns>Matched enemies</returns>
        IEnumerable<T> GetEnemiesInCombatWithParty<T>(Vector3 position, float distance) where T : WowUnit;

        /// <summary>
        /// Get all enemies that are in the area of a supplied path.
        /// </summary>
        /// <typeparam name="T">WowUnit or WowPlayer</typeparam>
        /// <param name="path">List of circle centers</param>
        /// <param name="distance">Radius of the area</param>
        /// <returns>Matched enemies</returns>
        IEnumerable<T> GetEnemiesInPath<T>(IEnumerable<Vector3> path, float distance) where T : WowUnit;

        /// <summary>
        /// Get all enemies that target a partymember in a certain area.
        /// </summary>
        /// <typeparam name="T">WowUnit or WowPlayer</typeparam>
        /// <param name="position">Center of the area</param>
        /// <param name="distance">Radius of the area</param>
        /// <returns>Matched enemies</returns>
        IEnumerable<T> GetEnemiesTargetingPartymembers<T>(Vector3 position, float distance) where T : WowUnit;

        /// <summary>
        /// Get all enemies in a certain area.
        /// </summary>
        /// <typeparam name="T">WowUnit or WowPlayer</typeparam>
        /// <param name="position">Center of the area</param>
        /// <param name="distance">Radius of the area</param>
        /// <returns>Matched enemies</returns>
        IEnumerable<T> GetNearEnemies<T>(Vector3 position, float distance) where T : WowUnit;

        /// <summary>
        /// Get all friends in a certain area.
        /// </summary>
        /// <typeparam name="T">WowUnit or WowPlayer</typeparam>
        /// <param name="position">Center of the area</param>
        /// <param name="distance">Radius of the area</param>
        /// <returns>Matched friends</returns>
        IEnumerable<T> GetNearFriends<T>(Vector3 position, float distance) where T : WowUnit;

        /// <summary>
        /// Get all partymembers in a certain area.
        /// </summary>
        /// <typeparam name="T">WowUnit or WowPlayer</typeparam>
        /// <param name="position">Center of the area</param>
        /// <param name="distance">Radius of the area</param>
        /// <returns>Matched partymembers</returns>
        IEnumerable<T> GetNearPartymembers<T>(Vector3 position, float distance) where T : WowUnit;

        /// <summary>
        /// Get a wow object by its guid.
        /// </summary>
        /// <typeparam name="T">Type of the WowObject, example WowPlayer or WowGameobject...</typeparam>
        /// <param name="guid">The guid to match</param>
        /// <returns>Matched wow object</returns>
        T GetWowObjectByGuid<T>(ulong guid) where T : WowObject;

        /// <summary>
        /// Get a unit by its name.
        /// </summary>
        /// <typeparam name="T"> WowUnit or WowPlayer</typeparam>
        /// <param name="name">Name of the unit</param>
        /// <param name="stringComparison">StringComparison to use</param>
        /// <returns>The matched unit or null if not found</returns>
        T GetWowUnitByName<T>(string name, StringComparison stringComparison = StringComparison.Ordinal) where T : WowUnit;

        /// <summary>
        /// Refresh the is world loaded status.
        /// </summary>
        /// <returns>Whether the world is loaded or not</returns>
        bool RefreshIsWorldLoaded();

        /// <summary>
        /// Get/Update all objects from the wow process.
        /// </summary>
        void UpdateWowObjects();
    }
}