using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Gigamud.Communications.Sockets.Telnet;
using Gigamud.Infrastructure.Formatter;

namespace Gigamud.UI.Console
{
    public partial class MainConsole : UserControl
    {
        TelnetSocket _socket;
        Run _currentRun;
        string _buffer;

        public MainConsole()
        {
            InitializeComponent();
            _buffer = string.Empty;
            _socket = new TelnetSocket("MajorMud.DontExist.com", 23);
            _socket.MessageRecieved += new TelnetSocket.IncomingMessageHandler(_socket_MessageRecieved);
            _socket.Connect();
        }

        void _socket_MessageRecieved(string message)
        {
            if (this.Dispatcher.CheckAccess())
            {
                _buffer += message;

                string[] lines = _buffer.Split('\n');
                int len = lines.Length;
                if (!message.EndsWith("\n") && !message.EndsWith(":"))
                {
                    len--;
                    _buffer = lines[lines.Length - 1];
                }
                else
                    _buffer = string.Empty;

                for (int i = 0; i < len; ++i)
                    foreach (Run r in TextFormatter.Format(i == lines.Length - 1 ? lines[i] : lines[i] + "\n"))
                        tblkConsole.Inlines.Add(r);

                Viewer.UpdateLayout();
                Viewer.ScrollToVerticalOffset(double.MaxValue);
            }
            else
                this.Dispatcher.BeginInvoke(() => { _socket_MessageRecieved(message); });
        }

        private void tbxMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_socket.IsConnected)
                    _socket.Write(tbxMain.Text);
                tbxMain.Text = string.Empty;
            }
        }
    }
}
