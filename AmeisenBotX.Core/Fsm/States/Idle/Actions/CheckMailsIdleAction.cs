using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public class CheckMailsIdleAction : IIdleAction
    {
        public CheckMailsIdleAction(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            Rnd = new Random();
        }

        public bool AutopilotOnly => true;

        public int MaxCooldown => 25 * 60 * 1000;

        public int MaxDuration => 3 * 60 * 1000;

        public int MinCooldown => 15 * 60 * 1000;

        public int MinDuration => 2 * 60 * 1000;

        private bool CheckedMails { get; set; }

        private Vector3 CurrentMailbox { get; set; }

        private DateTime MailboxCheckTime { get; set; }

        private Vector3 OriginPosition { get; set; }

        private bool ReturnedToOrigin { get; set; }

        private Random Rnd { get; }

        private WowInterface WowInterface { get; }

        public bool Enter()
        {
            CheckedMails = false;
            MailboxCheckTime = default;
            OriginPosition = WowInterface.Player.Position;

            if (WowInterface.Db.TryGetPointsOfInterest(WowInterface.ObjectManager.MapId, Data.Db.Enums.PoiType.Mailbox, WowInterface.Player.Position, 256.0f, out IEnumerable<Vector3> mailboxes))
            {
                CurrentMailbox = mailboxes.OrderBy(e => e.GetDistance(WowInterface.Player.Position)).First();
                return true;
            }

            return false;
        }

        public void Execute()
        {
            if (!CheckedMails)
            {
                if (CurrentMailbox.GetDistance(WowInterface.Player.Position) > 3.5f)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, CurrentMailbox);
                }
                else
                {
                    WowInterface.MovementEngine.StopMovement();

                    WowGameobject mailbox = WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>()
                        .FirstOrDefault(e => e.GameobjectType == WowGameobjectType.Mailbox && e.Position.GetDistance(CurrentMailbox) < 1.0f);

                    if (mailbox != null)
                    {
                        WowInterface.HookManager.WowObjectRightClick(mailbox);
                        WowInterface.HookManager.LuaDoString("for i=1,GetInboxNumItems()do AutoLootMailItem(i)end");
                    }

                    CheckedMails = true;
                    MailboxCheckTime = DateTime.UtcNow + TimeSpan.FromSeconds(Rnd.Next(7, 16));
                }
            }
            else if (!ReturnedToOrigin && MailboxCheckTime < DateTime.UtcNow)
            {
                if (CurrentMailbox.GetDistance(OriginPosition) > 8.0f)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, OriginPosition);
                }
                else
                {
                    WowInterface.MovementEngine.StopMovement();
                    ReturnedToOrigin = true;
                }
            }
        }
    }
}