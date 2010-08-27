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
using System.Collections.Generic;

namespace Gigamud.Communications.Sockets
{
    public class TelnetSocket
    {
        Socket _socket;
        EndPoint _serverEndPoint;
        bool waiting = false;

        static readonly int MAXBUFFER = 1024;
        byte[] _readbuffer;
        byte[] _writebuffer;

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
                SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();
                connectArgs.UserToken = _socket;
                connectArgs.RemoteEndPoint = _serverEndPoint;
                connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ConnectAsyncCallback);
                _socket.ConnectAsync(connectArgs);
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

            SocketAsyncEventArgs readArgs = new SocketAsyncEventArgs();
            byte[] readbuffer = new byte[MAXBUFFER];
            readArgs.SetBuffer(readbuffer, 0, MAXBUFFER);
            readArgs.UserToken = _socket;
            readArgs.RemoteEndPoint = _serverEndPoint;

            readArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessServerOptions);

            _socket.ReceiveAsync(readArgs);
        }

        void ProcessServerOptions(object sender, SocketAsyncEventArgs e)
        {
            if (e.ConnectByNameError != null)
                ; // handle error
            else if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    if ((e.Buffer[0] ^ 0xFF) > 0)
                    {
                        e.Completed -= new EventHandler<SocketAsyncEventArgs>(ProcessServerOptions);
                        e.Completed += new EventHandler<SocketAsyncEventArgs>(RecievedBytes);
                        RecievedBytes(sender, e);
                        return;
                    }

                    int idx = 0;
                    List<byte> response = new List<byte>();
                    while (idx < e.BytesTransferred && e.Buffer[idx++] == 0xFF)
                    {
                        response.Add(0xFF);  // add command marker
                        byte query = e.Buffer[idx++];
                        byte option = e.Buffer[idx++];

                        if (option == 0x3) // sga
                        {
                            if (query == 0xFD) // do
                                response.Add(0xFB); // will
                            else
                                response.Add(0xFD); // do
                        }
                        else if (query == 0xFD) // do
                            response.Add(0xFC); // wont
                        else
                            response.Add(0xFE); // dont
                    }

                    SocketAsyncEventArgs writeArgs = new SocketAsyncEventArgs();
                    writeArgs.UserToken = _socket;
                    writeArgs.RemoteEndPoint = _serverEndPoint;
                    byte[] writebuffer = response.ToArray();
                    writeArgs.SetBuffer(writebuffer, 0, writebuffer.Length);
                    writeArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ServerResponseHandler);

                    _socket.SendAsync(writeArgs);
                }
            }
        }

        void ServerResponseHandler(object sender, SocketAsyncEventArgs e)
        {
            if (e.ConnectByNameError != null)
                ; // handle error
            else if (e.SocketError == SocketError.Success)
            {
                string s = string.Empty;

                SocketAsyncEventArgs readArgs = new SocketAsyncEventArgs();
                byte[] readbuffer = new byte[MAXBUFFER];
                readArgs.SetBuffer(readbuffer, 0, MAXBUFFER);
                readArgs.UserToken = _socket;
                readArgs.RemoteEndPoint = _serverEndPoint;

                readArgs.Completed += new EventHandler<SocketAsyncEventArgs>(RecievedBytes);

                _socket.ReceiveAsync(readArgs);
            }
        }

        void RecievedBytes(object sender, SocketAsyncEventArgs e)
        {
            if (e.ConnectByNameError != null)
                ; // handle connection error
            else if (e.SocketError == SocketError.Success)
            {
                // process the message
                string msg = TelnetEncoding.ConvertFromBytes(e.Buffer, e.Offset, e.BytesTransferred);

                if (msg.Length > 0)
                {
                    // notify any listeners
                    if (MessageRecieved != null)
                        MessageRecieved(msg);
                }

                SocketAsyncEventArgs readArgs = new SocketAsyncEventArgs();
                byte[] readbuffer = new byte[MAXBUFFER];
                readArgs.SetBuffer(readbuffer, 0, MAXBUFFER);
                readArgs.UserToken = _socket;
                readArgs.RemoteEndPoint = _serverEndPoint;

                readArgs.Completed += new EventHandler<SocketAsyncEventArgs>(RecievedBytes);

                _socket.ReceiveAsync(readArgs);
                //}
                //else
                //    waiting = true;
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

        public void Write(string cmd)
        {
            SocketAsyncEventArgs writeArgs = new SocketAsyncEventArgs();
            writeArgs.UserToken = _socket;
            writeArgs.RemoteEndPoint = _serverEndPoint;
            writeArgs.Completed += new EventHandler<SocketAsyncEventArgs>(WriteCompleted);

            if (!cmd.EndsWith("\n"))
                cmd += "\n";

            byte[] b = new byte[cmd.Length];
            for (int i = 0; i < cmd.Length; ++i)
                b[i] = (byte)cmd[i];

            writeArgs.SetBuffer(b, 0, cmd.Length);

            _socket.SendAsync(writeArgs);
        }

        void WriteCompleted(object sender, SocketAsyncEventArgs e)
        {
        }

        public void Disconnect()
        {
            if (IsConnected)
                _socket.Dispose();
        }
    }
}
