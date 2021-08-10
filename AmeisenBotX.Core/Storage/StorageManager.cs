using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Core.Storage
{
    public class StorageManager
    {
        public StorageManager(string basePath, IEnumerable<string> partsToRemove = null)
        {
            BasePath = basePath;
            PartsToRemove = partsToRemove;

            Storeables = new();
        }

        public List<IStoreable> Storeables { get; private set; }

        private string BasePath { get; }

        private IEnumerable<string> PartsToRemove { get; }

        public void LoadAll()
        {
            foreach (IStoreable s in Storeables)
            {
                string fullPath = BuildPath(s);

                try
                {
                    string parent = Path.GetDirectoryName(fullPath);

                    if (!Directory.Exists(parent))
                    {
                        continue;
                    }

                    s.Load(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(fullPath), new() { AllowTrailingCommas = true, NumberHandling = JsonNumberHandling.AllowReadingFromString }));
                }
                catch (Exception ex)
                {
                    AmeisenLogger.I.Log("CombatClass", $"Failed to load {s.GetType().Name} ({fullPath}):\n{ex}", LogLevel.Error);
                }
            }
        }

        public void SaveAll()
        {
            foreach (IStoreable s in Storeables)
            {
                string fullPath = BuildPath(s);

                try
                {
                    Dictionary<string, object> data = s.Save();

                    if (data == null)
                    {
                        continue;
                    }

                    string parent = Path.GetDirectoryName(fullPath);

                    if (!Directory.Exists(parent))
                    {
                        Directory.CreateDirectory(parent);
                    }

                    File.WriteAllText(fullPath, JsonSerializer.Serialize(data, new() { WriteIndented = true }));
                }
                catch (Exception ex)
                {
                    AmeisenLogger.I.Log("CombatClass", $"Failed to save {s.GetType().Name} ({fullPath}):\n{ex}", LogLevel.Error);
                }
            }
        }

        private string BuildPath(IStoreable s)
        {
            string typePath = (s.GetType().FullName + ".json").ToLower();

            foreach (string rep in PartsToRemove)
            {
                typePath = typePath.Replace(rep.ToLower(), string.Empty);
            }

            return Path.Combine(BasePath, typePath);
        }
    }
}