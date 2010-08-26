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
using Gigamud.Communications.Sockets.Encodings;

namespace Gigamud.Communications.Sockets
{
    public class TelnetSocket
    {
        Socket _socket;
        EndPoint _serverEndPoint;
        SocketAsyncEventArgs _connectArgs, _readArgs, _writeArgs;

        static readonly int MAXBUFFER = 1024;
        byte[] _buffer;

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

        public delegate void TelnetSocketConnectedHandler(Exception fault);
        public event TelnetSocketConnectedHandler Connected;

        public delegate void TelnetSocketMessageRecievedHandler(string message);
        public event TelnetSocketMessageRecievedHandler MessageRecieved;

        void ConnectAsyncCallback(object sender, SocketAsyncEventArgs e)
        {
            if (Connected != null)
                Connected(e.ConnectByNameError);

            _readArgs = new SocketAsyncEventArgs();
            _buffer = new byte[MAXBUFFER];
            _readArgs.SetBuffer(_buffer, 0, MAXBUFFER);
            _readArgs.UserToken = _socket;
            _readArgs.RemoteEndPoint = _serverEndPoint;

            _readArgs.Completed += new EventHandler<SocketAsyncEventArgs>(RecievedBytes);

            _socket.ReceiveAsync(_readArgs);
        }

        void RecievedBytes(object sender, SocketAsyncEventArgs e)
        {
            if (e.ConnectByNameError != null)
                ; // handle connection error
            else if (e.SocketError == SocketError.Success)
            {
                // process the message
                string msg = TelnetEncoding.ConvertFromBytes(e.Buffer, e.Offset, e.BytesTransferred);

                // notify any listeners
                if (MessageRecieved != null)
                    MessageRecieved(msg);

                if (msg.Length > 0)
                    // keep listening
                    _socket.ReceiveAsync(e);
            }
            else
            {
                // handle the error
                switch (e.SocketError)
                {
                    default: break;
                }
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
                _socket.Dispose();
        }
    }
}
