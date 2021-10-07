using AmeisenBotX.Common.Utils;
using System.Collections.Generic;
using System.Text.Json;

namespace AmeisenBotX.Common.Storage
{
    public abstract class SimpleConfigurable : IStoreable
    {
        public Dictionary<string, dynamic> Configurables { get; protected set; } = new();

        public virtual void Load(Dictionary<string, JsonElement> objects)
        {
            if (objects.ContainsKey("Configurables"))
            {
                foreach (KeyValuePair<string, dynamic> x in objects["Configurables"].ToDyn())
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