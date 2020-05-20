using AmeisenBotX.Core.Battleground.Profiles;
using AmeisenBotX.Core.Battleground.States;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Battleground
{
    public class BattlegroundEngine
    {
        public BattlegroundEngine(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            ForceCombat = false;
        }

        public IBattlegroundProfile BattlegroundProfile { get; private set; }

        public KeyValuePair<BattlegroundState, BasicBattlegroundState> CurrentState { get; private set; }

        public bool ForceCombat { get; internal set; }

        public BattlegroundState LastState { get; private set; }

        private WowInterface WowInterface { get; }

        public void AllianceFlagWasDropped()
        {
            if (BattlegroundProfile.GetType() == typeof(WarsongGulchProfile))
            {
                ((WarsongGulchProfile)BattlegroundProfile).AllianceFlagWasDropped();
            }
        }

        public void AllianceFlagWasPickedUp(string playername)
        {
            if (BattlegroundProfile.GetType() == typeof(WarsongGulchProfile))
            {
                ((WarsongGulchProfile)BattlegroundProfile).AllianceFlagWasPickedUp(playername);
            }
        }

        public bool AttackNearEnemies(double range = 25)
        {
            IEnumerable<WowPlayer> nearEnemies = WowInterface.ObjectManager.GetNearEnemies<WowPlayer>(WowInterface.ObjectManager.Player.Position, range);

            if (nearEnemies.Count() > 0)
            {
                WowPlayer target = nearEnemies.FirstOrDefault(e => BotUtils.IsValidUnit(e));

                if (target != null)
                {
                    if (WowInterface.ObjectManager.Player.Position.GetDistance(target.Position) > 10)
                    {
                        WowInterface.MovementEngine.SetState(MovementEngineState.Moving, target.Position);
                        WowInterface.MovementEngine.Execute();
                        return true;
                    }
                    else
                    {
                        WowInterface.HookManager.TargetGuid(target.Guid);
                        WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
                        return true;
                    }
                }
            }

            return false;
        }

        public void Execute()
        {
            List<WowGameobject> flagObjects = WowInterface.ObjectManager.WowObjects
                .OfType<WowGameobject>()
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position)).ToList();

            if (BattlegroundProfile == null)
            {
                TryLoadProfile(WowInterface.ObjectManager.MapId);

                if (BattlegroundProfile != null)
                {
                    CurrentState = BattlegroundProfile.States.First();
                }
            }
            else
            {
                CurrentState.Value.Execute();
            }
        }

        public void HordeFlagWasDropped()
        {
            if (BattlegroundProfile.GetType() == typeof(WarsongGulchProfile))
            {
                ((WarsongGulchProfile)BattlegroundProfile).HordeFlagWasDropped();
            }
        }

        public void HordeFlagWasPickedUp(string playername)
        {
            if (BattlegroundProfile.GetType() == typeof(WarsongGulchProfile))
            {
                ((WarsongGulchProfile)BattlegroundProfile).HordeFlagWasPickedUp(playername);
            }
        }

        public void Reset()
        {
            BattlegroundProfile = null;
        }

        public bool TryLoadProfile(MapId mapId)
        {
            switch (mapId)
            {
                case MapId.AlteracValley:
                    // Alterac Valley
                    return false;

                case MapId.WarsongGulch:
                    BattlegroundProfile = new WarsongGulchProfile(WowInterface, this);
                    return true;

                case MapId.ArathiBasin:
                    // Arathi Basin
                    return false;

                case MapId.EyeOfTheStorm:
                    // Eye of the Storm
                    return false;

                case MapId.StrandOfTheAncients:
                    // Strand of the Ancients
                    return false;

                default:
                    return false;
            }
        }

        internal IEnumerable<WowGameobject> GetBattlegroundFlags(bool onlyEnemy = true)
        {
            return WowInterface.ObjectManager.WowObjects
                           .OfType<WowGameobject>()
                           .Where(e => onlyEnemy ? (!WowInterface.ObjectManager.Player.IsAlliance() && e.DisplayId == (int)GameobjectDisplayId.WsgAllianceFlag)
                                   || (WowInterface.ObjectManager.Player.IsAlliance() && e.DisplayId == (int)GameobjectDisplayId.WsgHordeFlag)
                               : e.DisplayId == (int)GameobjectDisplayId.WsgAllianceFlag || e.DisplayId == (int)GameobjectDisplayId.WsgHordeFlag);
        }

        internal void SetState(BattlegroundState state)
        {
            if (BattlegroundProfile == null || CurrentState.Key == state)
            {
                // we are already in this state
                return;
            }

            LastState = CurrentState.Key;
            CurrentState.Value.Exit();
            CurrentState = BattlegroundProfile.States.First(s => s.Key == state);
            CurrentState.Value.Enter();
        }
    }
}