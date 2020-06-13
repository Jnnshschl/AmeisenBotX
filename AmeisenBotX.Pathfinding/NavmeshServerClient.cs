using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Pathfinding.Enums;
using AmeisenBotX.Pathfinding.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace AmeisenBotX.Pathfinding
{
    public class NavmeshServerClient : IPathfindingHandler
    {
        public NavmeshServerClient(string ip, int port)
        {
            Ip = IPAddress.Parse(ip);
            Port = port;
            TcpClient = new TcpClient();

            ConnectionWatchdog = new Timer(1000);
            ConnectionWatchdog.Elapsed += CConnectionWatchdog;
            ConnectionWatchdog.Start();
        }

        public IPAddress Ip { get; }

        public bool IsConnected { get; private set; }

        public int Port { get; }

        public TcpClient TcpClient { get; private set; }

        private Timer ConnectionWatchdog { get; }

        private StreamReader Reader { get; set; }

        private StreamWriter Writer { get; set; }

        public bool CastMovementRay(int mapId, Vector3 start, Vector3 end)
        {
            if (TcpClient.Connected)
            {
                try
                {
                    string pathJson = SendPathRequest(mapId, start, end, PathRequestFlags.None, MovementType.CastMovementRay);

                    if (IsValidJson(pathJson))
                    {
                        return JsonConvert.DeserializeObject<List<Vector3>>(pathJson.Trim()).Count > 0;
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log("Pathfinding", $"CastMovementRay failed:\n{e}", LogLevel.Error);
                }
            }

            return false;
        }

        public void Disconnect()
        {
            ConnectionWatchdog.Stop();
            Reader.Close();
            Writer.Close();
            TcpClient.Close();
        }

        public List<Vector3> GetPath(int mapId, Vector3 start, Vector3 end)
        {
            if (TcpClient.Connected)
            {
                try
                {
                    string pathJson = SendPathRequest(mapId, start, end, PathRequestFlags.None, MovementType.MoveToPosition);

                    if (IsValidJson(pathJson))
                    {
                        return JsonConvert.DeserializeObject<List<Vector3>>(pathJson.Trim());
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log("Pathfinding", $"GetPath failed:\n{e}", LogLevel.Error);
                }
            }

            return new List<Vector3>();
        }

        public Vector3 MoveAlongSurface(int mapId, Vector3 start, Vector3 end)
        {
            if (TcpClient.Connected)
            {
                try
                {
                    string pathJson = SendPathRequest(mapId, start, end, PathRequestFlags.None, MovementType.MoveAlongSurface);

                    if (IsValidJson(pathJson))
                    {
                        return JsonConvert.DeserializeObject<List<Vector3>>(pathJson.Trim()).FirstOrDefault();
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log("Pathfinding", $"MoveAlongSurface failed:\n{e}", LogLevel.Error);
                }
            }

            return new Vector3();
        }

        private static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}"))
                || (strInput.StartsWith("[") && strInput.EndsWith("]")))
            {
                try
                {
                    JToken obj = JToken.Parse(strInput);
                    return true;
                }
                catch
                {
                    // ignored
                }
            }

            return false;
        }

        private void CConnectionWatchdog(object sender, ElapsedEventArgs e)
        {
            if (TcpClient == null || !TcpClient.Connected)
            {
                try
                {
                    TcpClient = new TcpClient();
                    TcpClient.Connect(Ip, Port);
                    Reader = new StreamReader(TcpClient.GetStream());
                    Writer = new StreamWriter(TcpClient.GetStream());
                }
                catch (ObjectDisposedException)
                {
                    TcpClient = new TcpClient();
                }
                catch
                {
                    // server is maybe not running or whatever
                }
            }

            if (TcpClient?.Client != null)
            {
                IsConnected = TcpClient.Connected;
            }
        }

        private string SendPathRequest(int mapId, Vector3 start, Vector3 end, PathRequestFlags pathRequestFlags, MovementType movementType)
        {
            string pathRequest = JsonConvert.SerializeObject(new PathRequest(start, end, mapId, pathRequestFlags, movementType));

            Writer.WriteLine(pathRequest + " &gt;");
            Writer.Flush();

            string pathJson = Reader.ReadLine().Replace("&gt;", string.Empty);
            return pathJson;
        }
    }
}