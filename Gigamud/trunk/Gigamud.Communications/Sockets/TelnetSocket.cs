using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Net.Sockets;

namespace Gigamud.Communications.Sockets
{
    public class TelnetSocket
    {
        Socket _socket;
        EndPoint _serverEndPoint;
        SocketAsyncEventArgs _connectArgs, _readArgs, _writeArgs;

        static readonly int MAXBUFFER = 1024;

        public TelnetSocket(string hostDns, int port)
        {
            _serverEndPoint = new DnsEndPoint(hostDns, port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect()
        {
            if (_socket == null)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (!_socket.Connected)
            {
                _connectArgs = new SocketAsyncEventArgs();
                _connectArgs.UserToken = _socket;
                _connectArgs.RemoteEndPoint = _serverEndPoint;
                _connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ConnectAsyncCallback);
                _socket.ConnectAsync(_connectArgs);
            }
        }

        public bool IsConnected { get { return _socket.Connected; } }

        public event EventHandler<Exception> Connected;

        void ConnectAsyncCallback(object sender, SocketAsyncEventArgs e)
        {
            if (Connected != null)
                Connected(this, e.ConnectByNameError);
        }

        public void Disconnect()
        {
            if (IsConnected)
                _socket.Dispose();
        }
    }
}
