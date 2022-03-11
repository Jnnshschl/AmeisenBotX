using System.Collections.Generic;
using System.Text.Json;

namespace AmeisenBotX.Common.Storage
{
    public interface IStoreable
    {
        /// <summary> Load all values from json into a dictionary. Convert the JsonElement to any
        /// object using the To<T>() or ToDyn() extension. </summary> <param name="objects">Loaded objects.</param>
        void Load(Dictionary<string, JsonElement> objects);

        /// <summary>
        /// Should return all object that you want to save.
        /// </summary>
        /// <returns>Objects to save.</returns>
        Dictionary<string, object> Save();
    }
}