using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow.Objects
{
    public interface IObjectProvider
    {
        /// <summary>
        /// Gets fired when we updated all objects successfully.
        /// </summary>
        event Action<IEnumerable<WowObject>> OnObjectUpdateComplete;

        /// <summary>
        /// Returns wow's camera information.
        /// </summary>
        RawCameraInfo Camera { get; }

        Vector3 CenterPartyPosition { get; }

        /// <summary>
        /// Contains the state of the game while in GlueXML start screen.
        /// </summary>
        string GameState { get; }

        bool IsTargetInLineOfSight { get; }

        bool IsWorldLoaded { get; }

        WowUnit LastTarget { get; }

        WowMapId MapId { get; }

        int ObjectCount { get; set; }

        WowUnit Partyleader { get; }

        IEnumerable<ulong> PartymemberGuids { get; }

        IEnumerable<WowUnit> Partymembers { get; }

        IEnumerable<ulong> PartyPetGuids { get; }

        IEnumerable<WowUnit> PartyPets { get; }

        WowUnit Pet { get; }

        WowPlayer Player { get; }

        IntPtr PlayerBase { get; }

        WowUnit Target { get; }

        WowUnit Vehicle { get; }

        IEnumerable<WowObject> WowObjects { get; }

        int ZoneId { get; }

        string ZoneName { get; }

        string ZoneSubName { get; }
    }
}