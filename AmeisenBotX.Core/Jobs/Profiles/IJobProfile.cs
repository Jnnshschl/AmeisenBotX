using AmeisenBotX.Core.Jobs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Jobs.Profiles
{
    public interface IJobProfile
    {
        public string Name { get; }

        public string Description { get; }

        public string Author { get; }

        public JobType JobType { get; }
    }
}
