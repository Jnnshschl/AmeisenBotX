using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.LoginHandler
{
    public interface ILoginHandler
    {
        bool Login(Process wowProcess, string username, string password, int characterSlot);
    }
}
