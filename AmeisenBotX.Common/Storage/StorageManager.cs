using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Common.Storage
{
    public class StorageManager
    {
        /// <summary>
        /// Helper class used to save configureable values in json files. Files
        /// will be named after their full class name (including namespace).
        /// </summary>
        /// <param name="basePath">Folder to save the json files in.</param>
        /// <param name="partsToRemove">
        /// Strings that are going to be removed from the final filename,
        /// use this to remove namespace parts from them.
        /// </param>
        public StorageManager(string basePath, IEnumerable<string> partsToRemove = null)
        {
            BasePath = basePath;
            PartsToRemove = partsToRemove;

            Storeables = new();
        }

        public List<IStoreable> Storeables { get; private set; }

        private string BasePath { get; }

        private IEnumerable<string> PartsToRemove { get; }

        public void Load(IStoreable s)
        {
            string fullPath = BuildPath(s);

            try
            {
                string parent = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(parent))
                {
                    return;
                }

                s.Load(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(fullPath), new() { AllowTrailingCommas = true, NumberHandling = JsonNumberHandling.AllowReadingFromString }));
            }
            catch
            {
                // AmeisenLogger.I.Log("CombatClass", $"Failed to load {s.GetType().Name} ({fullPath}):\n{ex}", LogLevel.Error);
            }
        }

        public void LoadAll()
        {
            foreach (IStoreable s in Storeables)
            {
                Load(s);
            }
        }

        public void Save(IStoreable s)
        {
            string fullPath = BuildPath(s);

            try
            {
                Dictionary<string, object> data = s.Save();

                if (data == null)
                {
                    return;
                }

                string parent = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(parent))
                {
                    Directory.CreateDirectory(parent);
                }

                File.WriteAllText(fullPath, JsonSerializer.Serialize(data, new() { WriteIndented = true }));
            }
            catch
            {
                // AmeisenLogger.I.Log("CombatClass", $"Failed to save {s.GetType().Name} ({fullPath}):\n{ex}", LogLevel.Error);
            }
        }

        public void SaveAll()
        {
            foreach (IStoreable s in Storeables)
            {
                Save(s);
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