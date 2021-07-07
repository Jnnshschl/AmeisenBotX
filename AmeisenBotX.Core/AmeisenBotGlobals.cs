namespace AmeisenBotX.Core
{
    public class AmeisenBotGlobals
    {
        /// <summary>
        /// Is set to true, the bot will start to attack its current target even if its friendly.
        /// </summary>
        public bool ForceCombat { get; set; } = false;

        /// <summary>
        /// If set to true, the bot wont fight, useful when you are mounted and want to travel without fighting.
        /// </summary>
        public bool IgnoreCombat { get; set; } = false;
    }
}