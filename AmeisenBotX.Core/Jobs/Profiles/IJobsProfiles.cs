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
        JobType JobType { get; }
    }
}
