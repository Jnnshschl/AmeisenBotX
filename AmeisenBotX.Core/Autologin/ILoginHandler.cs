using System.Diagnostics;

namespace AmeisenBotX.Core.Autologin
{
    public interface ILoginHandler
    {
        bool Login(Process wowProcess, string username, string password, int characterSlot);
    }
}