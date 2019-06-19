using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Kamsar.WebConsole
{
    public class UDPSocket
    {
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 8 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback recv = null;
        private bool _stopLogging = false;

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public void Server(string address, int port, IProgressStatus progress)
        {
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            Receive(progress);
        }

        public void Stop()
        {
            _stopLogging = true;
        }

        private void Receive(IProgressStatus progress)
        {
            _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                if (_stopLogging)
                {
                    _socket.Shutdown(SocketShutdown.Send);
                    _socket.Close();
                    return;
                }

                State so = (State)ar.AsyncState;
                int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);

                var message = Encoding.ASCII.GetString(so.buffer, 0, bytes);
                var messageType = MessageType.Info;

                if (message.Contains("level=\"DEBUG\""))
                {
                    messageType = MessageType.Debug;
                }
                else if (message.Contains("level=\"WARN\""))
                {
                    messageType = MessageType.Warning;
                }
                else if (message.Contains("level=\"ERROR\""))
                {
                    messageType = MessageType.Error;
                }

                //progress.ReportStatus($"RECV: {epFrom.ToString()}: {bytes}, {message}", messageType);
                var level = GetPropertyValue(message, "level");
                var timestamp = GetPropertyValue(message, "timestamp");
                var logger = GetPropertyValue(message, "logger");

                DateTime dt;
                DateTime.TryParse(timestamp, out dt);

                progress.ReportStatus($"{level} {dt.ToLocalTime()} {logger} | {message}", messageType);

            }, state);
        }

        private string GetPropertyValue(string source, string property)
        {
            property = property + "=\"";
            var pFrom = source.IndexOf(property, StringComparison.Ordinal) + property.Length;
            var pTo = source.IndexOf("\"", pFrom, StringComparison.Ordinal);

            return source.Substring(pFrom, pTo - pFrom);
        }
    }
}