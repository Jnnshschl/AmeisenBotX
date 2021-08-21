using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Grinding.Profiles;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Grinding
{
    public class DefaultGrindEngine : IGrindingEngine
    {
        public DefaultGrindEngine(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            Bot = bot;
            Config = config;

            UpdateEnemiesEvent = new(TimeSpan.FromMilliseconds(500));

            GrindingTree = new
            (
                new Annotator
                (
                    new Leaf(UpdateEnemies),
                    new Selector
                    (
                        () => Profile != null,
                        new Leaf(() => BtStatus.Success),
                        new Selector
                        (
                            () => Enemies.Any(),
                            // go fight the nearest enemie
                            new Leaf(() => MoveToPosition(Enemies.First().Position)),
                            // no enemies near, go search for them
                            new Leaf(() => BtStatus.Success)
                        )
                    )
                )
            );
        }

        public AmeisenBotInterfaces Bot { get; }

        public AmeisenBotConfig Config { get; }

        public IGrindingProfile Profile { get; set; }

        private IEnumerable<IWowUnit> Enemies { get; set; }

        private Tree GrindingTree { get; }

        private TimegatedEvent UpdateEnemiesEvent { get; }

        public void Execute()
        {
            GrindingTree.Tick();
        }

        public void LoadProfile(IGrindingProfile profile)
        {
            Profile = profile;
        }

        private BtStatus MoveToPosition(Vector3 position, MovementAction movementAction = MovementAction.Move)
        {
            if (position != Vector3.Zero && Bot.Player.DistanceTo(position) > 3.0f)
            {
                Bot.Movement.SetMovementAction(movementAction, position);
                return BtStatus.Ongoing;
            }

            return BtStatus.Success;
        }

        private BtStatus UpdateEnemies()
        {
            if (UpdateEnemiesEvent.Run())
            {
                Enemies = Bot.GetNearEnemiesOrNeutrals<IWowUnit>(Bot.Player.Position, 100.0f).OrderBy(e => Bot.Player.DistanceTo(e));
            }

            return BtStatus.Success;
        }
    }
}