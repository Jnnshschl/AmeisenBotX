using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Pathfinding.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
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
            return false; // SendCastRayRequest(mapId, start, end, MovementType.CastMovementRay);
        }

        public IEnumerable<Vector3> GetPath(int mapId, Vector3 start, Vector3 end)
        {
            return SendPathRequest(mapId, start, end, MovementType.FindPath, PathRequestFlags.CatmullRomSpline);
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
            return SendPathRequest(mapId, start, end, MovementType.MoveAlongSurface)[0];
        }

        private unsafe Vector3[] SendPathRequest(int mapId, Vector3 start, Vector3 end, MovementType movementType, PathRequestFlags pathRequestFlags = PathRequestFlags.None)
        {
            if (IsConnected)
            {
                byte[] response = null;

                try
                {
                    response = SendData(new PathRequest(mapId, start, end, pathRequestFlags, movementType), sizeof(PathRequest));

                    if (response != null && response.Length >= sizeof(Vector3))
                    {
                        int nodeCount = response.Length / sizeof(Vector3);
                        Vector3[] path = new Vector3[nodeCount];

                        fixed (byte* pResult = response)
                        fixed (Vector3* pPath = path)
                        {
                            Buffer.MemoryCopy(pResult, pPath, response.Length, response.Length);
                            return path;
                        }
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.I.Log("Pathfinding", $"SendPathRequest failed: {BotUtils.ByteArrayToString(response)}\n{e}", LogLevel.Error);
                }
            }

            return default;
        }

        private unsafe Vector3 BuildAndSendRandomPointRequest(int mapId, Vector3 start, float maxRadius)
        {
            if (IsConnected)
            {
                byte[] response = null;

                try
                {
                    response = SendData(new RandomPointRequest(mapId, start, maxRadius), sizeof(RandomPointRequest));

                    if (response != null && response.Length >= sizeof(Vector3))
                    {
                        fixed (byte* pResult = response)
                        {
                            return new Span<Vector3>(pResult, 1)[0];
                        }
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.I.Log("Pathfinding", $"BuildAndSendRandomPointRequest failed: {BotUtils.ByteArrayToString(response)}\n{e}", LogLevel.Error);
                }
            }

            return Vector3.Zero;
        }
    }
}