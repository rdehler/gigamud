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

using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Gigamud.Communications.Sockets.Telnet
{
    /* Process for connecting
     * -------------------------
     * 1. Create the socket and connect.
     * 2. Negotiate options
     *      a. Server sends options it will/wont do
     *      b. Client responds with actions it will/won't support
     *      c. Option for Subnegotation
     *      d. Complete Subnegotation
     * 3. Connection is established and client can communicate with server 
     */

    /// <summary>
    /// Manages a Telnet Encoded TCP Socket.
    /// </summary>
    public class TelnetSocket
    {
        Socket _socket;
        EndPoint _address;

        string _dnsHost;
        int _port;

        #region Constructors

        public TelnetSocket()
        {
            _dnsHost = string.Empty;
            _port = -1;
        }

        public TelnetSocket(string dnsAddress)
        {
            _dnsHost = dnsAddress;
            _port = 23;
        }

        public TelnetSocket(string dnsAddress, int port)
        {
            _dnsHost = dnsAddress;
            _port = port;
        }

        #endregion

        #region Properties

        public bool IsConnected
        {
            get
            {
                return _socket == null || _socket.Connected;
            }
        }

        public string DnsHost
        {
            get
            {
                return _dnsHost;
            }
            set
            {
                if (!IsConnected)
                    _dnsHost = value;
                else
                    throw new InvalidOperationException("Cannot change the address on an active socket connection");
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                if (!IsConnected)
                    _port = value;
                else
                    throw new InvalidOperationException("Cannot change the port on an active socket connection");
            }
        }

        #endregion

        #region Events

        public delegate void IncomingMessageHandler(string message);
        public event IncomingMessageHandler MessageRecieved;

        #endregion

        #region Public Methods

        public void Connect()
        {
            if (_port < 0 || string.IsNullOrEmpty(_dnsHost))
                throw new InvalidOperationException("The socket has not been fully configured for communication!");

            if (_socket == null)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (_socket.Connected)
                throw new InvalidOperationException("Cannot connect to a socket that is already open");

            _address = new DnsEndPoint(_dnsHost, _port);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.UserToken = _socket;
            args.RemoteEndPoint = _address;
            args.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessSocketEvent);

            _socket.ConnectAsync(args);
        }

        public void Write(string message)
        {
            // format the message for telnet
            message = message.Trim() + "\r\n";

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.UserToken = _socket;
            args.RemoteEndPoint = _address;
            args.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessSocketEvent);

            byte[] buffer = new byte[message.Length];
            for (int i = 0; i < buffer.Length; ++i)
                buffer[i] = (byte)message[i];

            args.SetBuffer(buffer, 0, buffer.Length);

            _socket.SendAsync(args);
        }

        #endregion

        #region SocketEventHandlers

        void ProcessSocketEvent(object sender, SocketAsyncEventArgs e)
        {
            if (e.ConnectByNameError != null)
            {
                // there was a problem connecting to the host
                return;
            }

            if (e.SocketError == SocketError.Success)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.UserToken = _socket;
                args.RemoteEndPoint = _address;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessSocketEvent);

                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Connect:
                        // begin reading from the stream
                        try
                        {
                            args.SetBuffer(new byte[1024], 0, 1024);
                            _socket.ReceiveAsync(args);
                        }
                        catch (Exception ex)
                        {
                        }
                        break;
                    case SocketAsyncOperation.Send:
                        // start listening again
                        args.SetBuffer(new byte[1024], 0, 1024);
                        _socket.ReceiveAsync(args);
                        break;
                    case SocketAsyncOperation.Receive:
                        byte[] response;
                        string text = ProcessStream(e.Buffer, e.Offset, e.BytesTransferred, out response);

                        if (response.Length > 0) // need to write the response
                        {
                            args.SetBuffer(response, 0, response.Length);
                            _socket.SendAsync(args);
                        }
                        else // keep listening
                        {
                            args.SetBuffer(new byte[1024], 0, 1024);
                            _socket.ReceiveAsync(args);
                        }

                        if (MessageRecieved != null)
                            MessageRecieved(text);

                        break;
                    default:
                        break;
                }
            }
            else
            {
                // handle the socket error
            }
        }

        string ProcessStream(byte[] bytes, int offset, int bytesTransmitted, out byte[] response)
        {
            response = new byte[0];
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < bytesTransmitted; ++i)
            {
                int n = i + offset;
                byte b = bytes[n];

                // check for a telnet command
                if (TelnetCommands.IAC == (TelnetCommands)b)
                {
                    byte[] resp = ProcessCommand(bytes, n + 1);
                    if (resp.Length > 0)
                    {
                        Array.Resize<byte>(ref response, response.Length + resp.Length);
                        resp.CopyTo(response, response.Length - resp.Length);
                    }
                    i += 2;
                }
                else // process it normally
                {
                    char c = (char)b;
                    if (c != '\r' && c != '\b') // these mess with the text
                        sb.Append(c);
                }
            }

            return sb.ToString();
        }

        #endregion

        #region Telnet Negotation

        byte[] ProcessCommand(byte[] source, int offset)
        {
            byte[] response = new byte[] { (byte)TelnetCommands.IAC, (byte)TelnetCommands.NoOp, (byte)TelnetCommands.NoOp };

            TelnetCommands command = (TelnetCommands)source[offset];
            TelnetOptions option = (TelnetOptions)source[offset + 1];

            // note that we really only care about SGA at this point
            switch (command)
            {
                case TelnetCommands.Will: // the server is willing to enable this
                    if (option == TelnetOptions.SupressGoAhead ||
                        option == TelnetOptions.Echo)
                        response[1] = (byte)TelnetCommands.Do;
                    else
                        response[1] = (byte)TelnetCommands.Dont;
                    break;
                case TelnetCommands.Wont: // the server refuses to enable this
                    response[1] = (byte)TelnetCommands.Dont;
                    break;
                case TelnetCommands.Do: // the server requests you do this
                    if (option == TelnetOptions.SupressGoAhead ||
                        option == TelnetOptions.Echo)
                        response[1] = (byte)TelnetCommands.Will;
                    else
                        response[1] = (byte)TelnetCommands.Wont;
                    break;
                case TelnetCommands.Dont: // the server doesn't want you to do this
                    response[1] = (byte)TelnetCommands.Wont;
                    break;
                default:
                    response[1] = (byte)TelnetCommands.Wont;
                    break; // ignore these cases unless required
            }

            response[2] = (byte)option;

            return response;
        }

        #endregion
    }
}
