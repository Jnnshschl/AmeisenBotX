using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Cache.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public class CheckMailsIdleAction : IIdleAction
    {
        public CheckMailsIdleAction(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            Rnd = new Random();
        }

        public bool AutopilotOnly => true;

        public DateTime Cooldown { get; set; }

        public int MaxCooldown => 7 * 60 * 1000;

        public int MaxDuration => 2 * 60 * 1000;

        public int MinCooldown => 6 * 60 * 1000;

        public int MinDuration => 1 * 60 * 1000;

        private AmeisenBotInterfaces Bot { get; }

        private bool CheckedMails { get; set; }

        private Vector3 CurrentMailbox { get; set; }

        private DateTime MailboxCheckTime { get; set; }

        private Vector3 OriginPosition { get; set; }

        private bool ReturnedToOrigin { get; set; }

        private Random Rnd { get; }

        public bool Enter()
        {
            CheckedMails = false;
            MailboxCheckTime = default;
            OriginPosition = Bot.Player.Position;

            if (Bot.Db.TryGetPointsOfInterest(Bot.Objects.MapId, PoiType.Mailbox, Bot.Player.Position, 256.0f, out IEnumerable<Vector3> mailboxes))
            {
                CurrentMailbox = mailboxes.OrderBy(e => e.GetDistance(Bot.Player.Position)).First();
                return true;
            }

            return false;
        }

        public void Execute()
        {
            if (!CheckedMails)
            {
                if (CurrentMailbox.GetDistance(Bot.Player.Position) > 3.5f)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, CurrentMailbox);
                }
                else
                {
                    Bot.Movement.StopMovement();

                    IWowGameobject mailbox = Bot.Objects.WowObjects.OfType<IWowGameobject>()
                        .FirstOrDefault(e => e.GameobjectType == WowGameobjectType.Mailbox && e.Position.GetDistance(CurrentMailbox) < 1.0f);

                    if (mailbox != null)
                    {
                        Bot.Wow.InteractWithObject(mailbox.BaseAddress);
                        Bot.Wow.LuaDoString("for i=1,GetInboxNumItems()do AutoLootMailItem(i)end");
                    }

                    CheckedMails = true;
                    MailboxCheckTime = DateTime.UtcNow + TimeSpan.FromSeconds(Rnd.Next(7, 16));
                }
            }
            else if (!ReturnedToOrigin && MailboxCheckTime < DateTime.UtcNow)
            {
                if (CurrentMailbox.GetDistance(OriginPosition) > 8.0f)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, OriginPosition);
                }
                else
                {
                    Bot.Movement.StopMovement();
                    ReturnedToOrigin = true;
                }
            }
        }
    }
}