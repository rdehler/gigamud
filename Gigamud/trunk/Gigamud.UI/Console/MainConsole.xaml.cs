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

namespace Gigamud.UI.Console
{
    public partial class MainConsole : UserControl
    {
        TelnetSocket _socket;
        Run _currentRun;

        public MainConsole()
        {
            InitializeComponent();
            _socket = new TelnetSocket("69.205.222.240", 23);
            _socket.MessageRecieved += new TelnetSocket.IncomingMessageHandler(_socket_MessageRecieved);
            _socket.Connect();
        }

        void _socket_MessageRecieved(string message)
        {
            if (this.Dispatcher.CheckAccess())
            {
                Run r = new Run();
                r.Text = message;
                r.Foreground = new SolidColorBrush(Colors.Gray);
                if (tblkConsole.Inlines.Count == 0)
                    tblkConsole.Inlines.Add(new Run());
                tblkConsole.Inlines.Insert(tblkConsole.Inlines.Count - 1, r);
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
