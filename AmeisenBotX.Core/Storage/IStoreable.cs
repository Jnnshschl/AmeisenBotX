using System.Collections.Generic;
using System.Text.Json;

namespace AmeisenBotX.Core.Storage
{
    public interface IStoreable
    {
        void Load(Dictionary<string, JsonElement> objects);

        Dictionary<string, object> Save();
    }
}