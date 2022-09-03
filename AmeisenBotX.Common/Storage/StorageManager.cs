using AmeisenBotX.Common.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Common.Storage
{
    public class StorageManager
    {
        /// <summary>
        /// Helper class used to save configureable values in json files. Files will be named after
        /// their full class name (including namespace).
        /// </summary>
        /// <param name="basePath">Folder to save the json files in.</param>
        /// <param name="partsToRemove">
        /// Strings that are going to be removed from the final filename, use this to remove
        /// namespace parts from them.
        /// </param>
        public StorageManager(string basePath, IEnumerable<string> partsToRemove = null)
        {
            BasePath = basePath;
            PartsToRemove = partsToRemove;

            Storeables = new();
        }

        private string BasePath { get; }

        private IEnumerable<string> PartsToRemove { get; }

        private List<IStoreable> Storeables { get; set; }

        public void Load(IStoreable s)
        {
            if (!Storeables.Contains(s))
            {
                Register(s);
            }

            string fullPath = BuildPath(s);

            try
            {
                string parent = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(parent))
                {
                    return;
                }

                s.Load(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(fullPath), new JsonSerializerOptions() { AllowTrailingCommas = true, NumberHandling = JsonNumberHandling.AllowReadingFromString }));
            }
            catch
            {
                // AmeisenLogger.I.Log("CombatClass", $"Failed to load {s.GetType().Name}
                // ({fullPath}):\n{ex}", LogLevel.Error);
            }
        }

        public void LoadAll()
        {
            foreach (IStoreable s in Storeables)
            {
                Load(s);
            }
        }

        public void Register(IStoreable s)
        {
            Storeables.Add(s);
        }

        public void Save(IStoreable s)
        {
            if (!Storeables.Contains(s))
            {
                Register(s);
            }

            string fullPath = BuildPath(s);

            try
            {
                Dictionary<string, object> data = s.Save();

                if (data == null)
                {
                    return;
                }

                IOUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(fullPath));
                File.WriteAllText(fullPath, JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true }));
            }
            catch
            {
                // AmeisenLogger.I.Log("CombatClass", $"Failed to save {s.GetType().Name}
                // ({fullPath}):\n{ex}", LogLevel.Error);
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