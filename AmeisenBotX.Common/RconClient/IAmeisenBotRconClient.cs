using AmeisenBotX.RconClient.Enums;
using AmeisenBotX.RconClient.Messages;
using System.Collections.Generic;

namespace AmeisenBotX.RconClient
{
    public interface IAmeisenBotRconClient
    {
        string Endpoint { get; set; }
        string Guid { get; set; }
        bool NeedToRegister { get; }
        List<ActionType> PendingActions { get; }
        RegisterMessage RegisterMessage { get; }

        bool KeepAlive();
        bool PullPendingActions();
        bool Register();
        bool SendData(DataMessage dataMessage);
        bool SendImage(string image);
    }
}