using AmeisenBotX.Common.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Common.Storage
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
    public class StorageManager(string basePath, IEnumerable<string> partsToRemove = null)
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            AllowTrailingCommas = true,
            WriteIndented = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        private string BasePath { get; } = basePath;

        private IEnumerable<string> PartsToRemove { get; } = partsToRemove;

        private List<IStoreable> Storeables { get; set; } = [];

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

                s.Load(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(fullPath), Options));
            }
            catch { }
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
                File.WriteAllText(fullPath, JsonSerializer.Serialize(data, Options));
            }
            catch { }
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