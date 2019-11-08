using System.Diagnostics;

namespace AmeisenBotX.Core.LoginHandler
{
    public interface ILoginHandler
    {
        bool Login(Process wowProcess, string username, string password, int characterSlot);
    }
}
