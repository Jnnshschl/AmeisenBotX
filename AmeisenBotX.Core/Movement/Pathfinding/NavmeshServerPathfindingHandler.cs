using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Pathfinding.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace AmeisenBotX.Core.Movement.Pathfinding
{
    public class NavmeshServerPathfindingHandler : CustomTcpClient, IPathfindingHandler
    {
        public NavmeshServerPathfindingHandler(string ip, int port) : base(ip, port)
        {
        }

        public bool CastMovementRay(int mapId, Vector3 start, Vector3 end)
        {
            return BuildAndSendPathRequest<List<Vector3>>(mapId, start, end, MovementType.CastMovementRay).Count() > 0;
        }

        public List<Vector3> GetPath(int mapId, Vector3 start, Vector3 end)
        {
            return BuildAndSendPathRequest<List<Vector3>>(mapId, start, end, MovementType.MoveToPosition);
        }

        public Vector3 MoveAlongSurface(int mapId, Vector3 start, Vector3 end)
        {
            List<Vector3> path = BuildAndSendPathRequest<List<Vector3>>(mapId, start, end, MovementType.MoveAlongSurface);

            if (path == null || path.Count == 0)
            {
                return Vector3.Zero;
            }

            return path.FirstOrDefault();
        }

        private T BuildAndSendPathRequest<T>(int mapId, Vector3 start, Vector3 end, MovementType movementType, PathRequestFlags pathRequestFlags = PathRequestFlags.None)
        {
            if (TcpClient.Connected)
            {
                try
                {
                    string pathJson = SendPathRequest(mapId, start, end, pathRequestFlags, movementType);

                    if (BotUtils.IsValidJson(pathJson))
                    {
                        return JsonConvert.DeserializeObject<T>(pathJson.Trim());
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log("Pathfinding", $"BuildAndSendPathRequest failed:\n{e}", LogLevel.Error);
                }
            }

            return default;
        }

        private string SendPathRequest(int mapId, Vector3 start, Vector3 end, PathRequestFlags pathRequestFlags, MovementType movementType)
        {
            return SendRequest(JsonConvert.SerializeObject(new PathRequest(start, end, mapId, pathRequestFlags, movementType))).GetAwaiter().GetResult();
        }
    }
}