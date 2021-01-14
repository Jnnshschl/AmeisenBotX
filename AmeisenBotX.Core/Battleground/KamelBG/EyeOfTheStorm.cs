using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Battleground.KamelBG
{
    internal class EyeOfTheStorm : IBattlegroundEngine
    {
        private WowInterface wowInterface;

        public EyeOfTheStorm(WowInterface wowInterface)
        {
            this.wowInterface = wowInterface;
        }

        public string Author => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public void Enter()
        {
            throw new NotImplementedException();
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }

        public void Leave()
        {
            throw new NotImplementedException();
        }
    }
}
