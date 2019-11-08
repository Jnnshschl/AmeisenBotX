using AmeisenBotX.Pathfinding.Enums;
using AmeisenBotX.Pathfinding.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
                    string pathRequest = JsonConvert.SerializeObject(new PathRequest(start, end, mapId, PathRequestFlags.None));

                    Writer.WriteLine(pathRequest + " &gt;");
                    Writer.Flush();

                    string pathJson = Reader.ReadLine().Replace("&gt;", string.Empty);

                    if (IsValidJson(pathJson))
                    {
                        return JsonConvert.DeserializeObject<List<Vector3>>(pathJson.Trim());
                    }
                }
                catch
                {
                    // ignored
                }
            }

            return new List<Vector3>();
        }

        public bool IsInLineOfSight(Vector3 start, Vector3 end, int mapId)
        {
            return GetPath(mapId, start, end).Count == 1;
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
            if (TcpClient == null)
            {
                TcpClient = new TcpClient();
            }

            if (TcpClient == null || !TcpClient.Connected)
            {
                try
                {
                    TcpClient.Connect(Ip, Port);
                    Reader = new StreamReader(TcpClient.GetStream());
                    Writer = new StreamWriter(TcpClient.GetStream());
                }
                catch (ObjectDisposedException)
                {
                    TcpClient = new TcpClient();
                }
                catch (Exception)
                {
                    // server is maybe not running or whatever
                }
            }

            if (TcpClient?.Client != null)
            {
                IsConnected = TcpClient.Connected;
            }
        }
    }
}
