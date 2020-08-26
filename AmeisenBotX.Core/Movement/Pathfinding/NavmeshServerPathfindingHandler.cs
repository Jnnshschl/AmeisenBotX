using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Pathfinding.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement.Pathfinding
{
    public class NavmeshServerPathfindingHandler : CustomTcpClient, IPathfindingHandler
    {
        public NavmeshServerPathfindingHandler(string ip, int port) : base(ip, port)
        {
        }

        public bool CastMovementRay(int mapId, Vector3 start, Vector3 end)
        {
            return BuildAndSendPathRequest<Vector3>(mapId, start, end, MovementType.CastMovementRay) != default;
        }

        public List<Vector3> GetPath(int mapId, Vector3 start, Vector3 end)
        {
            return BuildAndSendPathRequest<List<Vector3>>(mapId, start, end, MovementType.FindPath, PathRequestFlags.CatmullRomSpline);
        }

        public Vector3 GetRandomPoint(int mapId)
        {
            return BuildAndSendRandomPointRequest(mapId, Vector3.Zero, 0f);
        }

        public Vector3 GetRandomPointAround(int mapId, Vector3 start, float maxRadius)
        {
            return BuildAndSendRandomPointRequest(mapId, start, maxRadius);
        }

        public Vector3 MoveAlongSurface(int mapId, Vector3 start, Vector3 end)
        {
            return BuildAndSendPathRequest<Vector3>(mapId, start, end, MovementType.MoveAlongSurface);
        }

        private T BuildAndSendPathRequest<T>(int mapId, Vector3 start, Vector3 end, MovementType movementType, PathRequestFlags pathRequestFlags = PathRequestFlags.None)
        {
            if (IsConnected)
            {
                string response = string.Empty;

                try
                {
                    response = SendString(1, JsonConvert.SerializeObject(new PathRequest(mapId, start, end, pathRequestFlags, movementType)));

                    if (BotUtils.IsValidJson(response))
                    {
                        return JsonConvert.DeserializeObject<T>(response);
                    }
                    else
                    {
                        AmeisenLogger.I.Log("Pathfinding", $"BuildAndSendPathRequest no valid JSON response: {response}", LogLevel.Error);
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.I.Log("Pathfinding", $"BuildAndSendPathRequest failed: {response}\n{e}", LogLevel.Error);
                }
            }

            return default;
        }

        private Vector3 BuildAndSendRandomPointRequest(int mapId, Vector3 start, float maxRadius)
        {
            if (IsConnected)
            {
                string response = string.Empty;

                try
                {
                    response = SendString(2, JsonConvert.SerializeObject(new RandomPointRequest(mapId, start, maxRadius)));

                    if (BotUtils.IsValidJson(response))
                    {
                        return JsonConvert.DeserializeObject<Vector3>(response);
                    }
                    else
                    {
                        AmeisenLogger.I.Log("Pathfinding", $"BuildAndSendRandomPointRequest no valid JSON response: {response}", LogLevel.Error);
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.I.Log("Pathfinding", $"BuildAndSendRandomPointRequest failed: {response}\n{e}", LogLevel.Error);
                }
            }

            return Vector3.Zero;
        }
    }
}