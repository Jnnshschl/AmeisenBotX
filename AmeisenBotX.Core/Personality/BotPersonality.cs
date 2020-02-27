using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Personality.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AmeisenBotX.Core.Personality
{
    [Serializable]
    public class BotPersonality
    {
        public BotPersonality(string path)
        {
            FilePath = path;
            Clear();
        }

        public Scale Bravery => (Scale)((int)Math.Round(BraveryScore));

        public double BraveryScore { get; set; } = 2.0;

        public string FilePath { get; }

        public Dictionary<ulong, double> RememberedPlayers { get; set; }

        public Dictionary<ulong, double> RememberedUnits { get; set; }

        public Scale Sociality => (Scale)((int)Math.Round(SocialityScore));

        public double SocialityScore { get; set; } = 2.0;

        public void Clear()
        {
            RememberedUnits = new Dictionary<ulong, double>();
            RememberedPlayers = new Dictionary<ulong, double>();
        }

        public PlayerRelationship GetPlayerRelationship(WowPlayer player, bool rememberIfUnknown = true)
        {
            if (RememberedUnits.ContainsKey(player.Guid))
            {
                return (PlayerRelationship)((int)Math.Round(RememberedUnits[player.Guid]));
            }
            else if (rememberIfUnknown)
            {
                RememberedUnits.Add(player.Guid, 0);
            }

            return PlayerRelationship.Unknown;
        }

        public UnitRelationship GetUnitRelationship(WowUnit unit, bool rememberIfUnknown = true)
        {
            if (RememberedUnits.ContainsKey(unit.Guid))
            {
                return (UnitRelationship)((int)Math.Round(RememberedUnits[unit.Guid]));
            }
            else if (rememberIfUnknown)
            {
                RememberedUnits.Add(unit.Guid, 0);
            }

            return UnitRelationship.Unknown;
        }

        public void Load()
        {
            if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            }

            if (File.Exists(FilePath))
            {
                using Stream stream = File.Open(FilePath, FileMode.Open);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                BotPersonality loadedCache = (BotPersonality)binaryFormatter.Deserialize(stream);

                try
                {
                    if (loadedCache != null)
                    {
                        RememberedUnits = loadedCache.RememberedUnits ?? new Dictionary<ulong, double>();
                        RememberedPlayers = loadedCache.RememberedPlayers ?? new Dictionary<ulong, double>();
                        SocialityScore = loadedCache.SocialityScore;
                        BraveryScore = loadedCache.BraveryScore;
                    }
                    else
                    {
                        Clear();
                    }
                }
                catch
                {
                    stream.Close();
                    File.Delete(FilePath);
                    Clear();
                }
            }
        }

        public void Save()
        {
            if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            }

            using Stream stream = File.Open(FilePath, FileMode.Create);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, this);
        }
    }
}