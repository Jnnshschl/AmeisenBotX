using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Battleground.KamelBG.Enums;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Battleground.KamelBG
{
    internal class EyeOfTheStorm : IBattlegroundEngine
    {
        public EyeOfTheStorm(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            CaptureFlagEvent = new(TimeSpan.FromSeconds(1));
            CombatEvent = new(TimeSpan.FromSeconds(2));
        }

        public string Author => "Lukas";

        public string Description => "Eye of the Storm";

        public string Name => "Eye of the Storm";

        public List<Vector3> PathBase { get; } = new List<Vector3>()
        {
            new Vector3(2284, 1731, 1189),//Mage Tower
            new Vector3(2286, 1402, 1197),//Draenei Ruins
            new Vector3(2048, 1393, 1194),//Blood Elf Tower
            new Vector3(2043, 1729, 1189)//Fel Reaver Ruins
        };

        public List<Vector3> PathFlag { get; } = new List<Vector3>()
        {
            new Vector3(2176, 1570, 1159)//Flag
        };

        private AmeisenBotInterfaces Bot { get; }

        private TimegatedEvent CaptureFlagEvent { get; }

        private TimegatedEvent CombatEvent { get; }

        private int CurrentNodeCounter { get; set; }

        private string FactionFlagState { get; set; }

        public void Combat()
        {
            IWowPlayer weakestPlayer = Bot.GetNearEnemies<IWowPlayer>(Bot.Player.Position, 30.0f).OrderBy(e => e.Health).FirstOrDefault();

            if (weakestPlayer != null)
            {
                double distance = weakestPlayer.Position.GetDistance(Bot.Player.Position);
                double threshold = Bot.CombatClass.IsMelee ? 3.0 : 28.0;

                if (distance > threshold)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, weakestPlayer.Position);
                }
                else if (CombatEvent.Run())
                {
                    // StateMachine.Get<StateCombat>().Mode = CombatMode.Force;
                    Bot.Wow.ChangeTarget(weakestPlayer.Guid);
                }
            }
            else
            {
            }
        }

        public void Enter()
        {
            Faction();
        }

        public void Execute()
        {
            Combat();

            IWowGameobject FlagNode = Bot.Objects.WowObjects
                .OfType<IWowGameobject>()
                .Where(x => Enum.IsDefined(typeof(Flags), x.DisplayId)
                        && x.Position.GetDistance(Bot.Player.Position) < 15)
                .OrderBy(x => x.Position.GetDistance(Bot.Player.Position))
                .FirstOrDefault();

            if (FlagNode != null)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, FlagNode.Position);

                if (Bot.Player.Position.GetDistance(FlagNode.Position) <= 4)
                {
                    Bot.Movement.StopMovement();

                    if (CaptureFlagEvent.Run())
                    {
                        Bot.Wow.InteractWithObject(FlagNode);
                    }
                }
                else
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, FlagNode.Position);
                }
            }
            else
            {
                if (Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\" for i = 1, GetNumMapLandmarks(), 1 do base, status = GetMapLandmarkInfo(i) {v:0}= {v:0}..base..\":\"..status..\";\" end"), out string result))
                {
                    Vector3 currentNode = PathBase[CurrentNodeCounter];
                    string[] AllBaseList = result.Split(';');

                    if (Bot.Player.HasBuffById(34976))
                    {
                        if (AllBaseList[CurrentNodeCounter].Contains(FactionFlagState))
                        {
                            Bot.Movement.SetMovementAction(MovementAction.Move, currentNode);
                        }
                        else
                        {
                            ++CurrentNodeCounter;
                            if (CurrentNodeCounter >= PathBase.Count)
                            {
                                CurrentNodeCounter = 0;
                            }
                        }
                    }
                    else
                    {
                        if (AllBaseList[CurrentNodeCounter].Contains("Uncontrolled")
                            || AllBaseList[CurrentNodeCounter].Contains("In Conflict")
                            || AllBaseList[CurrentNodeCounter].Contains(FactionFlagState))
                        {
                            Bot.Movement.SetMovementAction(MovementAction.Move, currentNode);
                        }

                        if (Bot.Player.Position.GetDistance(currentNode) < 10.0f)
                        {
                            ++CurrentNodeCounter;

                            if (CurrentNodeCounter >= PathBase.Count)
                            {
                                CurrentNodeCounter = 0;
                            }
                        }
                        else if (FactionFlagState != null && AllBaseList[CurrentNodeCounter].Contains(FactionFlagState))
                        {
                            ++CurrentNodeCounter;
                            if (CurrentNodeCounter >= PathBase.Count)
                            {
                                CurrentNodeCounter = 0;
                            }
                        }
                        else if (FlagNode != null)
                        {
                            IEnumerable<IWowPlayer> enemiesNearFlag = Bot.GetNearEnemies<IWowPlayer>(FlagNode.Position, 40);
                            IEnumerable<IWowPlayer> friendsNearFlag = Bot.GetNearFriends<IWowPlayer>(FlagNode.Position, 40);
                            IEnumerable<IWowPlayer> friendsNearPlayer = Bot.GetNearFriends<IWowPlayer>(Bot.Player.Position, 20);

                            if (enemiesNearFlag != null)
                            {
                                if (enemiesNearFlag.Count() >= 2)
                                {
                                    if (friendsNearFlag != null && (friendsNearFlag.Any() || friendsNearPlayer.Any()))
                                    {
                                        Bot.Movement.SetMovementAction(MovementAction.Move, currentNode);
                                        return;
                                    }
                                }
                                else
                                {
                                    Bot.Movement.SetMovementAction(MovementAction.Move, currentNode);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            Bot.Movement.SetMovementAction(MovementAction.Move, currentNode);
                        }
                    }
                }
            }
        }

        public void Faction()
        {
            if (!Bot.Player.IsHorde())
            {
                FactionFlagState = "Alliance Controlled";
            }
            else
            {
                FactionFlagState = "Hord Controlled";
            }
        }

        public void Reset()
        {
        }
    }
}