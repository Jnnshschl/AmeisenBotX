using AmeisenBotX.Common.Utils;
using System.Collections.Generic;
using System.Text.Json;

namespace AmeisenBotX.Common.Storage
{
    public abstract class SimpleConfigurable : IStoreable
    {
        public Dictionary<string, dynamic> Configurables { get; protected set; } = [];

        public virtual void Load(Dictionary<string, JsonElement> objects)
        {
            if (objects.TryGetValue("Configurables", out JsonElement value))
            {
                foreach (KeyValuePair<string, dynamic> x in value.ToDyn())
                {
                    if (Configurables.ContainsKey(x.Key))
                    {
                        Configurables[x.Key] = x.Value;
                    }
                    else
                    {
                        Configurables.Add(x.Key, x.Value);
                    }
                }
            }
        }

        public virtual Dictionary<string, object> Save()
        {
            return new()
            {
                { "Configurables", Configurables }
            };
        }
    }
}