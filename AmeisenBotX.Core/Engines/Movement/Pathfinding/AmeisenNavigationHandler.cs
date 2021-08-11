using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Pathfinding.Enums;
using AnTCP.Client;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AmeisenBotX.Core.Engines.Movement.Pathfinding
{
    public class AmeisenNavigationHandler : IPathfindingHandler
    {
        public AmeisenNavigationHandler(string ip, int port)
        {
            Client = new(ip, port);
            ConnectionWatchdog = new(ObserveConnection);
            ConnectionWatchdog.Start();
        }

        private AnTcpClient Client { get; }

        private Thread ConnectionWatchdog { get; }

        private bool ShouldExit { get; set; }

        public IEnumerable<Vector3> GetPath(int mapId, Vector3 origin, Vector3 target)
        {
            try
            {
                return Client.IsConnected ? Client.Send((byte)EMessageType.PATH, (mapId, origin, target, 2)).AsArray<Vector3>() : Array.Empty<Vector3>();
            }
            catch
            {
                return Array.Empty<Vector3>();
            }
        }

        public Vector3 GetRandomPoint(int mapId)
        {
            try
            {
                return Client.IsConnected ? Client.Send((byte)EMessageType.RANDOM_POINT, mapId).As<Vector3>() : Vector3.Zero;
            }
            catch
            {
                return Vector3.Zero;
            }
        }

        public Vector3 GetRandomPointAround(int mapId, Vector3 origin, float maxRadius)
        {
            try
            {
                return Client.IsConnected ? Client.Send((byte)EMessageType.RANDOM_POINT_AROUND, (mapId, origin, maxRadius)).As<Vector3>() : Vector3.Zero;
            }
            catch
            {
                return Vector3.Zero;
            }
        }

        public Vector3 MoveAlongSurface(int mapId, Vector3 origin, Vector3 target)
        {
            try
            {
                return Client.IsConnected ? Client.Send((byte)EMessageType.MOVE_ALONG_SURFACE, (mapId, origin, target)).As<Vector3>() : Vector3.Zero;
            }
            catch
            {
                return Vector3.Zero;
            }
        }

        public void Stop()
        {
            ShouldExit = true;
            ConnectionWatchdog.Join();
        }

        private void ObserveConnection()
        {
            while (!ShouldExit)
            {
                if (!Client.IsConnected)
                {
                    try
                    {
                        Client.Connect();
                    }
                    catch
                    {
                        // ignored, will happen when we cant connect
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}