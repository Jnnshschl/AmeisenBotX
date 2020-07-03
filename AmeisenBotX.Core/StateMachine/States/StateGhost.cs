using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateGhost : BasicState
    {
        public StateGhost(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            PortalSearchEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(Config.GhostPortalSearchMs));
        }

        public Vector3 CorpsePosition { get; private set; }

        public bool NeedToEnterPortal { get; private set; }

        public TimegatedEvent PortalSearchEvent { get; private set; }

        public override void Enter()
        {
            // WowUnit spiritHealer = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Name.ToUpper().Contains("SPIRIT HEALER"));
            //
            // if (spiritHealer != null)
            // {
            //     WowInterface.HookManager.UnitOnRightClick(spiritHealer);
            // }
        }

        public override void Execute()
        {
            if (WowInterface.ObjectManager.Player.Health > 1)
            {
                StateMachine.SetState((int)BotState.Idle);
            }

            if (!NeedToEnterPortal)
            {
                // first step, determine our corpse/portal position
                if (StateMachine.IsBattlegroundMap(WowInterface.ObjectManager.MapId))
                {
                    // we are on a battleground just wait for the mass ress
                    return;
                }
                else if (StateMachine.IsDungeonMap(StateMachine.LastDiedMap)
                         && WowInterface.DungeonEngine?.Profile != null)
                {
                    CorpsePosition = WowInterface.DungeonEngine.Profile.WorldEntry;
                }
                else
                {
                    WowInterface.XMemory.ReadStruct(WowInterface.OffsetList.CorpsePosition, out Vector3 corpsePosition);
                    CorpsePosition = corpsePosition;
                }

                if (PortalSearchEvent.Run())
                {
                    // search for nearby portals
                    WowGameobject nearestPortal = WowInterface.ObjectManager.WowObjects
                        .OfType<WowGameobject>()
                        .Where(e => e.DisplayId == (int)GameobjectDisplayId.DungeonPortalNormal || e.DisplayId == (int)GameobjectDisplayId.DungeonPortalHeroic)
                        .FirstOrDefault(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.GhostPortalScanThreshold);

                    if (nearestPortal != null)
                    {
                        CorpsePosition = BotUtils.MoveAhead(nearestPortal.Position, BotMath.GetFacingAngle2D(WowInterface.ObjectManager.Player.Position, nearestPortal.Position), 6);
                        NeedToEnterPortal = true;
                    }
                    else
                    {
                        CorpsePosition = StateMachine.LastDiedPosition;
                        NeedToEnterPortal = false;
                    }
                }
            }

            // step two, move to the corpse/portal
            if (WowInterface.ObjectManager.Player.Position.GetDistance(CorpsePosition) > Config.GhostResurrectThreshold)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, CorpsePosition);
            }
            else
            {
                if (NeedToEnterPortal)
                {
                    // move into portal, MoveAhead is used to go beyond the portals entry point to make sure enter it
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, CorpsePosition);
                }
                else
                {
                    // if we died normally, just resurrect
                    WowInterface.HookManager.RetrieveCorpse();
                }
            }
        }

        public override void Exit()
        {
            CorpsePosition = default;
            NeedToEnterPortal = false;
        }
    }
}