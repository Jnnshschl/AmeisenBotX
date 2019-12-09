using AmeisenBotX.Core.Personality.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Personality
{
    public class BotPersonality
    {
        public Scale Sociality { get; set; }

        public double SocialityScore { get; set; } = 2.0;

        public Scale Bravery { get; set; }

        public double BraveryScore { get; set; } = 2.0;

        public Scale Wisdom { get; set; }

        public double WisdomScore { get; set; } = 2.0;

        public Scale Troll { get; set; }

        public double TrollScore { get; set; } = 2.0;
    }
}
