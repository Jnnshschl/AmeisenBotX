using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace AmeisenBotX.Common.Utils
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

        protected BinaryReader Reader { get; private set; }

        protected NetworkStream Stream { get; private set; }

        protected TcpClient TcpClient { get; private set; }

        private int ConnectionFailedCounter { get; set; }

        public void Disconnect()
        {
            ConnectionWatchdog.Stop();
            TcpClient.Close();
        }

        public unsafe byte[] SendData<T>(T data, int size) where T : unmanaged
        {
            if (Stream == null)
            {
                return null;
            }

            Stream.Write(BitConverter.GetBytes(size));
            Stream.Write(new Span<byte>(&data, size));
            Stream.Flush();

            int dataSize = BitConverter.ToInt32(Reader.ReadBytes(4), 0);
            return Reader.ReadBytes(dataSize);
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
                    TcpClient = new() { NoDelay = true };
                    TcpClient.ConnectAsync(Ip, Port).Wait();

                    if (TcpClient.Client.Connected)
                    {
                        Stream = TcpClient.GetStream();
                        Stream.ReadTimeout = 1000;
                        Stream.WriteTimeout = 1000;

                        Reader = new(Stream);

                        ConnectionFailedCounter = 0;
                    }
                }
                else
                {
                    IsConnected = Stream != null && Reader != null && SendData(0, 4)?[0] == 1;

                    if (!IsConnected)
                    {
                        ++ConnectionFailedCounter;
                    }
                    else
                    {
                        ConnectionFailedCounter = 0;
                    }
                }
            }
            catch
            {
                ++ConnectionFailedCounter;
            }
            finally
            {
                if (ConnectionFailedCounter > 3)
                {
                    TcpClient.Close();
                    TcpClient.Dispose();
                    TcpClient = null;

                    IsConnected = false;
                }

                connectionTimerBusy = 0;
            }
        }

        private void Init(IPAddress ip, int port, int watchdogPollMs)
        {
            Ip = ip;
            Port = port;

            ConnectionWatchdog = new(watchdogPollMs);
            ConnectionWatchdog.Elapsed += ConnectionWatchdogTick;
            ConnectionWatchdog.Start();
        }
    }
}