using AmeisenBotX.Learning.Sessions.Combat;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Learning
{
    public class AmeisenBotLearner
    {
        public AmeisenBotLearner()
        {
            SpellUsageCombatSessions = new();
        }

        public List<SpellUsageCombatSession> SpellUsageCombatSessions { get; set; }

        public static AmeisenBotLearner FromJson(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    return JsonSerializer.Deserialize<AmeisenBotLearner>(File.ReadAllText(filename), new() { AllowTrailingCommas = true, NumberHandling = JsonNumberHandling.AllowReadingFromString });
                }
                catch { }
            }

            return new();
        }

        public void Save(string filename)
        {
            try
            {
                File.WriteAllText(filename, JsonSerializer.Serialize(this));
            }
            catch { }
        }
    }
}