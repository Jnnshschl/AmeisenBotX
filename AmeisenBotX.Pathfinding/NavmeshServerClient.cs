using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace AmeisenBotX.Pathfinding
{
    public class NavmeshServerClient : IPathfindingHandler
    {
        public IPAddress Ip { get; }
        public int Port { get; }
        public TcpClient TcpClient { get; }
        public bool IsConnected { get; private set; }

        private Timer ConnectionWatchdog { get; }

        public NavmeshServerClient(string ip, int port)
        {
            Ip = IPAddress.Parse(ip);
            Port = port;
            TcpClient = new TcpClient();

            ConnectionWatchdog = new Timer(1000);
            ConnectionWatchdog.Elapsed += CConnectionWatchdog;
            ConnectionWatchdog.Start();
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
                    // Server not running
                }
            }

            if (TcpClient?.Client != null)
            {
                IsConnected = TcpClient.Connected;
            }
        }

        public List<WowPosition> GetPath(int mapId, WowPosition start, WowPosition end)
        {
            if (!TcpClient.Connected)
            {
                return new List<WowPosition>();
            }

            StreamReader sReader = new StreamReader(TcpClient.GetStream(), Encoding.ASCII);
            StreamWriter sWriter = new StreamWriter(TcpClient.GetStream(), Encoding.ASCII);

            string pathRequest = JsonConvert.SerializeObject(new PathRequest(mapId, start, end));

            sWriter.WriteLine(pathRequest + " &gt;");
            sWriter.Flush();

            string pathJson = sReader.ReadLine().Replace("&gt;", "");

            if (IsValidJson(pathJson))
            {
                return JsonConvert.DeserializeObject<List<WowPosition>>(pathJson.Trim());
            }
            else
            {
                return new List<WowPosition>();
            }
        }

        private static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) ||
                (strInput.StartsWith("[") && strInput.EndsWith("]")))
            {
                try
                {
                    JToken obj = JToken.Parse(strInput);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool IsInLineOfSight(WowPosition start, WowPosition end, int mapId)
        {
            return GetPath(mapId, start, end).Count == 1;
        }

        public void Disconnect()
        {
            ConnectionWatchdog.Stop();
            TcpClient.Close();
        }
    }
}