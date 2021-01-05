using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.States.Idle.Actions
{
    public class CheckMailsIdleAction : IIdleAction
    {
        public CheckMailsIdleAction()
        {
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

        public bool Enter()
        {
            CheckedMails = false;
            MailboxCheckTime = default;
            OriginPosition = WowInterface.I.ObjectManager.Player.Position;

            if (WowInterface.I.Db.TryGetPointsOfInterest(WowInterface.I.ObjectManager.MapId, Data.Db.Enums.PoiType.Mailbox, WowInterface.I.ObjectManager.Player.Position, 256.0, out IEnumerable<Vector3> mailboxes))
            {
                CurrentMailbox = mailboxes.OrderBy(e => e.GetDistance(WowInterface.I.ObjectManager.Player.Position)).First();
                return true;
            }

            return false;
        }

        public void Execute()
        {
            if (!CheckedMails)
            {
                if (CurrentMailbox.GetDistance(WowInterface.I.ObjectManager.Player.Position) > 3.5f)
                {
                    WowInterface.I.MovementEngine.SetMovementAction(MovementAction.Move, CurrentMailbox);
                }
                else
                {
                    WowInterface.I.MovementEngine.StopMovement();

                    WowGameobject mailbox = WowInterface.I.ObjectManager.WowObjects.OfType<WowGameobject>()
                        .FirstOrDefault(e => e.GameobjectType == WowGameobjectType.Mailbox && e.Position.GetDistance(CurrentMailbox) < 1.0f);

                    if (mailbox != null)
                    {
                        WowInterface.I.HookManager.WowObjectRightClick(mailbox);
                        WowInterface.I.HookManager.LuaDoString("for i=1,GetInboxNumItems()do AutoLootMailItem(i)end");
                    }

                    CheckedMails = true;
                    MailboxCheckTime = DateTime.UtcNow + TimeSpan.FromSeconds(Rnd.Next(7, 16));
                }
            }
            else if (!ReturnedToOrigin && MailboxCheckTime < DateTime.UtcNow)
            {
                if (CurrentMailbox.GetDistance(OriginPosition) > 8.0f)
                {
                    WowInterface.I.MovementEngine.SetMovementAction(MovementAction.Move, OriginPosition);
                }
                else
                {
                    WowInterface.I.MovementEngine.StopMovement();
                    ReturnedToOrigin = true;
                }
            }
        }
    }
}