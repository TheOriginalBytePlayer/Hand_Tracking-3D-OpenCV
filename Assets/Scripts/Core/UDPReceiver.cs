using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HandTrackingCore
{
    /// <summary>
    /// Core UDP receiver that is independent of Unity.
    /// Receives UDP data on a specified port and invokes a callback when data is received.
    /// </summary>
    public class UDPReceiver : IDisposable
    {
        private const int THREAD_JOIN_TIMEOUT_MS = 1000;
        
        private Thread receiveThread;
        private UdpClient client;
        private bool isReceiving;
        
        public int Port { get; private set; }
        public bool PrintToConsole { get; set; }
        
        /// <summary>
        /// Event fired when data is received
        /// </summary>
        public event Action<string> OnDataReceived;
        
        /// <summary>
        /// Event fired when an error occurs
        /// </summary>
        public event Action<Exception> OnError;

        public UDPReceiver(int port = 5032)
        {
            Port = port;
            PrintToConsole = false;
        }

        /// <summary>
        /// Start receiving UDP data
        /// </summary>
        public void StartReceiving()
        {
            if (isReceiving)
                return;
                
            isReceiving = true;
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        /// <summary>
        /// Stop receiving UDP data
        /// </summary>
        public void StopReceiving()
        {
            isReceiving = false;
            
            if (client != null)
            {
                client.Close();
                client = null;
            }
            
            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Join(THREAD_JOIN_TIMEOUT_MS);
                // Note: Thread.Abort() is obsolete in modern .NET
                // The thread will terminate when the loop exits after setting isReceiving = false
            }
        }

        private void ReceiveData()
        {
            client = new UdpClient(Port);
            
            while (isReceiving)
            {
                try
                {
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] dataByte = client.Receive(ref anyIP);
                    string data = Encoding.UTF8.GetString(dataByte);

                    if (PrintToConsole)
                    {
                        Console.WriteLine(data);
                    }
                    
                    OnDataReceived?.Invoke(data);
                }
                catch (Exception err)
                {
                    if (isReceiving) // Only report errors if we're supposed to be receiving
                    {
                        if (PrintToConsole)
                        {
                            Console.WriteLine(err.ToString());
                        }
                        OnError?.Invoke(err);
                    }
                }
            }
        }

        public void Dispose()
        {
            StopReceiving();
        }
    }
}
