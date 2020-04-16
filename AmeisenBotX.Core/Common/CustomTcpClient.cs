using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace AmeisenBotX.Core.Common
{
    public abstract class CustomTcpClient
    {
        public CustomTcpClient(string ip, int port, int watchdogPollMs = 1000)
            => Init(IPAddress.Parse(ip), port, watchdogPollMs);

        public CustomTcpClient(IPAddress ip, int port, int watchdogPollMs = 1000)
            => Init(ip, port, watchdogPollMs);

        public IPAddress Ip { get; private set; }

        public bool IsConnected { get; private set; }

        public int Port { get; private set; }

        protected Timer ConnectionWatchdog { get; private set; }

        protected StreamReader Reader { get; set; }

        protected TcpClient TcpClient { get; private set; }

        protected StreamWriter Writer { get; set; }

        public void Disconnect()
        {
            ConnectionWatchdog.Stop();
            Reader.Close();
            Writer.Close();
            TcpClient.Close();
        }

        public string SendRequest(string payload)
        {
            Writer.WriteLine($"{payload}&gt;");
            Writer.Flush();
            return Reader.ReadLine().Replace("&gt;", string.Empty);
        }

        private void ConnectionWatchdogTick(object sender, ElapsedEventArgs e)
        {
            if (TcpClient == null)
            {
                TcpClient = new TcpClient();
            }
            else if (!TcpClient.Connected)
            {
                try
                {
                    TcpClient.Connect(Ip, Port);

                    Reader = new StreamReader(TcpClient.GetStream(), Encoding.ASCII);
                    Writer = new StreamWriter(TcpClient.GetStream(), Encoding.ASCII);
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
            else if (TcpClient.Client != null)
            {
                IsConnected = TcpClient.Connected;
            }
        }

        private void Init(IPAddress ip, int port, int watchdogPollMs)
        {
            Ip = ip;
            Port = port;
            TcpClient = new TcpClient();

            ConnectionWatchdog = new Timer(watchdogPollMs);
            ConnectionWatchdog.Elapsed += ConnectionWatchdogTick;
            ConnectionWatchdog.Start();
        }
    }
}