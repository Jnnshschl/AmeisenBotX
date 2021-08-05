using AmeisenBotX.Core.Keyboard;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Engines.Movement.Settings
{
    public class KeyBindingSettings
    {
        #region Public

        /// <summary>
        /// Key bindings for starting or stopping the bot.
        /// </summary>
        public (VirtualKeyStates, Keys) StartStopBot { get; set; } = (VirtualKeyStates.LALT, Keys.X);

        #endregion
    }
}
