using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NeuroGeometry.Abstractions;
using Newtonsoft.Json; // Requires Newtonsoft.Json NuGet package

namespace NeuroGeometry.Services
{
    public class ThinkGearService : IBiosensor
    {
        private TcpClient _client;
        private Stream _stream;
        private bool _isRunning;
        private const string HOST = "127.0.0.1";
        private const int PORT = 13854;

        // "Biosensor in its second epoch" tracking
        private long _epochCycle = 0;
        private bool _hasDetectedIndependentlyDifferential = false;

        public event Action<EegData> DataReceived;

        // Add this line inside the class
        public event Action<string> ErrorOccurred;

        // Replace the Connect() method with this version:
        public void Connect()
        {
            Task.Run(() =>
            {
                try
                {
                    _client = new TcpClient();
                    // Try to connect to the ThinkGear Connector
                    _client.Connect(HOST, PORT);
                    _stream = _client.GetStream();

                    // Send Configuration
                    string config = @"{""enableRawOutput"": true, ""format"": ""Json""}";
                    byte[] cmd = Encoding.ASCII.GetBytes(config);
                    _stream.Write(cmd, 0, cmd.Length);

                    _isRunning = true;
                    ReadStream();
                }
                catch (Exception ex)
                {
                    // NEW: Report the error to the UI!
                    ErrorOccurred?.Invoke("Connection Failed: Is TGC Running?");
                    Console.WriteLine(ex.Message);
                }
            });
        }

        private void ReadStream()
        {
            using (StreamReader reader = new StreamReader(_stream))
            {
                while (_isRunning && _client.Connected)
                {
                    try
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;

                        // Parse logic
                        var packet = JsonConvert.DeserializeObject<EegData>(line);

                        if (packet != null && packet.eSense != null)
                        {
                            _epochCycle++;

                            // Check signal quality
                            if (packet.PoorSignalLevel < 50)
                                _hasDetectedIndependentlyDifferential = true;

                            DataReceived?.Invoke(packet);
                        }
                    }
                    catch { /* Swallow serialization errors in the stream */ }
                }
            }
        }

        public void Disconnect()
        {
            _isRunning = false;
            _stream?.Close();
            _client?.Close();
        }
    }
}