using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace AmeisenBotX.Core.Common
{
    public abstract class CustomTcpClient
    {
        private int connectionTimerBusy;

        public CustomTcpClient(string ip, int port, int watchdogPollMs = 1000)
        {
            Init(IPAddress.Parse(ip), port, watchdogPollMs);
        }

        public CustomTcpClient(IPAddress ip, int port, int watchdogPollMs = 1000)
        {
            Init(ip, port, watchdogPollMs);
        }

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

        public async Task<string> SendRequest(string payload)
        {
            await Writer.WriteLineAsync($"{payload}&gt;");
            await Writer.FlushAsync();
            return Reader.ReadLineAsync().GetAwaiter().GetResult().Replace("&gt;", string.Empty);
        }

        private async void ConnectionWatchdogTick(object sender, ElapsedEventArgs e)
        {
            // only start one timer tick at a time
            if (Interlocked.CompareExchange(ref connectionTimerBusy, 1, 0) == 1)
            {
                return;
            }

            try
            {
                if (TcpClient == null)
                {
                    TcpClient = new TcpClient()
                    {
                        NoDelay = true
                    };
                }
                else if (!TcpClient.Connected)
                {
                    try
                    {
                        await TcpClient.ConnectAsync(Ip, Port);

                        Reader = new StreamReader(TcpClient.GetStream(), Encoding.ASCII);
                        Writer = new StreamWriter(TcpClient.GetStream(), Encoding.ASCII);
                    }
                    catch (ObjectDisposedException)
                    {
                        TcpClient = new TcpClient()
                        {
                            NoDelay = true
                        };
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
            finally
            {
                connectionTimerBusy = 0;
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