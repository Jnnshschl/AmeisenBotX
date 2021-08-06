using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Core.Keyboard
{
    public class KeybindingConverter : JsonConverter<(VirtualKeyStates, Keys)>
    {
        public override (VirtualKeyStates, Keys) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            (VirtualKeyStates, Keys) keyBinding = (VirtualKeyStates.NONE, Keys.None);

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                string propName = reader.GetString();
                reader.Read();

                switch (propName)
                {
                    case "Alt":
                        keyBinding.Item1 = (VirtualKeyStates)Enum.Parse(typeof(VirtualKeyStates), reader.GetString());
                        break;

                    case "Key":
                        keyBinding.Item2 = (Keys)Enum.Parse(typeof(Keys), reader.GetString());
                        break;
                }
            }

            return keyBinding;
        }

        public override void Write(Utf8JsonWriter writer, (VirtualKeyStates, Keys) value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Alt", value.Item1.ToString());
            writer.WriteString("Key", value.Item2.ToString());
            writer.WriteEndObject();
        }
    }

    [Serializable]
    public class KeyBindingSettings
    {
        /// <summary>
        /// Key binding for starting or stopping the bot.
        /// </summary>
        [JsonConverter(typeof(KeybindingConverter))]
        public (VirtualKeyStates, Keys) StartStopBot { get; set; } = (VirtualKeyStates.LALT, Keys.X);
    }
}