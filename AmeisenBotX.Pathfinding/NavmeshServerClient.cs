using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public TcpClient TcpClient { get; }

        private Timer ConnectionWatchdog { get; }

        public void Disconnect()
        {
            ConnectionWatchdog.Stop();
            TcpClient.Close();
        }

        public List<Vector3> GetPath(int mapId, Vector3 start, Vector3 end)
        {
            if (!TcpClient.Connected)
            {
                return new List<Vector3>();
            }

            using (StreamReader reader = new StreamReader(TcpClient.GetStream(), Encoding.ASCII))
            using (StreamWriter writer = new StreamWriter(TcpClient.GetStream(), Encoding.ASCII))
            {
                string pathRequest = JsonConvert.SerializeObject(new PathRequest(start, end, mapId));

                writer.WriteLine(pathRequest + " &gt;");
                writer.Flush();

                string pathJson = reader.ReadLine().Replace("&gt;", string.Empty);

                if (IsValidJson(pathJson))
                {
                    return JsonConvert.DeserializeObject<List<Vector3>>(pathJson.Trim());
                }
                else
                {
                    return new List<Vector3>();
                }
            }
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
            if (!TcpClient.Connected)
            {
                try
                {
                    TcpClient.Connect(Ip, Port);
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
    }
}