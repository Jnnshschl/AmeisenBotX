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
            ConnectionFailedCounter = 100;
        }

        public CustomTcpClient(IPAddress ip, int port, int watchdogPollMs = 1000)
        {
            Init(ip, port, watchdogPollMs);
            ConnectionFailedCounter = 100;
        }

        public IPAddress Ip { get; private set; }

        public bool IsConnected { get; private set; }

        public int Port { get; private set; }

        protected Timer ConnectionWatchdog { get; private set; }

        protected StreamReader Reader { get; set; }

        protected TcpClient TcpClient { get; private set; }

        protected StreamWriter Writer { get; set; }

        private int ConnectionFailedCounter { get; set; }

        public void Disconnect()
        {
            ConnectionWatchdog.Stop();
            Reader.Close();
            Writer.Close();
            TcpClient.Close();
        }

        public string SendString(int msgType, string payload)
        {
            Writer.WriteLineAsync($"{msgType}{payload}").Wait();
            Writer.FlushAsync().Wait();
            return Reader.ReadLineAsync().Result;
        }

        private void ConnectionWatchdogTick(object sender, ElapsedEventArgs e)
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
                    TcpClient = new TcpClient() { NoDelay = true };

                    TcpClient.ConnectAsync(Ip, Port).Wait();

                    if (TcpClient.Client.Connected)
                    {
                        NetworkStream stream = TcpClient.GetStream();

                        Reader = new StreamReader(stream, Encoding.ASCII);
                        Writer = new StreamWriter(stream, Encoding.ASCII);
                    }
                }
                else
                {
                    IsConnected = Reader != null && Writer != null && SendString(0, "1")?.Length > 0;

                    if (!IsConnected)
                    {
                        ++ConnectionFailedCounter;
                    }
                    else
                    {
                        ConnectionFailedCounter = 0;
                    }

                    if (ConnectionFailedCounter > 3)
                    {
                        TcpClient.Close();
                        TcpClient.Dispose();
                        TcpClient = null;

                        IsConnected = false;
                    }
                }
            }
            catch
            {
                // server is maybe not running or whatever
                TcpClient.Close();
                TcpClient.Dispose();
                TcpClient = null;

                IsConnected = false;
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