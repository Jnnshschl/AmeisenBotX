using AmeisenBotX.Common.Utils;
using System.Collections.Generic;
using System.Text.Json;

namespace AmeisenBotX.Common.Storage
{
    public abstract class SimpleConfigureable : IStoreable
    {
        public Dictionary<string, dynamic> Configureables { get; protected set; } = new();

        public virtual void Load(Dictionary<string, JsonElement> objects)
        {
            if (objects.ContainsKey("Configureables"))
            {
                foreach (KeyValuePair<string, dynamic> x in objects["Configureables"].ToDyn())
                {
                    if (Configureables.ContainsKey(x.Key))
                    {
                        Configureables[x.Key] = x.Value;
                    }
                    else
                    {
                        Configureables.Add(x.Key, x.Value);
                    }
                }
            }
        }

        public virtual Dictionary<string, object> Save()
        {
            return new()
            {
                { "Configureables", Configureables }
            };
        }
    }
}