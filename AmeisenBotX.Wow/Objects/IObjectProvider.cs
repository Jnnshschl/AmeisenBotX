using AmeisenBotX.Common.Math;
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
        event Action<IEnumerable<IWowObject>> OnObjectUpdateComplete;

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

        IWowUnit LastTarget { get; }

        WowMapId MapId { get; }

        int ObjectCount { get; set; }

        IWowUnit Partyleader { get; }

        IEnumerable<ulong> PartymemberGuids { get; }

        IEnumerable<IWowUnit> Partymembers { get; }

        IEnumerable<ulong> PartyPetGuids { get; }

        IEnumerable<IWowUnit> PartyPets { get; }

        IWowUnit Pet { get; }

        IWowPlayer Player { get; }

        IntPtr PlayerBase { get; }

        IWowUnit Target { get; }

        IWowUnit Vehicle { get; }

        IEnumerable<IWowObject> WowObjects { get; }

        int ZoneId { get; }

        string ZoneName { get; }

        string ZoneSubName { get; }
    }
}