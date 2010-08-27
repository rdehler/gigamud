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
using System.ComponentModel;
using Gigamud.Infrastructure.Utilities;
using Gigamud.Communications.Sockets;
using System.Text;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using Gigamud.Communications.Sockets.Telnet;

namespace Gigamud.UI.TelnetConsole
{
    // TODO: Need a better way to hook the dispatcher.  also need to add base viewmodel with this functionality and propertychanged, etc.
    public class TelnetConsoleViewModel : INotifyPropertyChanged
    {
        string _serverName;
        public string ServerName
        {
            get
            {
                return _serverName;
            }
            set
            {
                _serverName = value;
                FirePropertyChanged("ServerName");
            }
        }

        int _port;
        public string Port
        {
            get
            {
                return _port.ToString();
            }
            set
            {
                if (int.TryParse(value, out _port))
                    FirePropertyChanged("Port");
            }
        }

        string _command;
        public string Command
        {
            get
            {
                return _command;
            }
            set
            {
                _command = value;
                FirePropertyChanged("Command");
            }
        }

        ObservableCollection<string> _content;
        public ObservableCollection<string> Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;
                FirePropertyChanged("Content");
            }
        }

        public ICommand ConnectCommand { get; set; }
        public ICommand SendCommand { get; set; }

        TelnetSocket _commSocket;
        Dispatcher _appDispatcher;

        public TelnetConsoleViewModel()
        {
            _serverName = "time-a.nist.gov";
            _port = 13;
            _content = new ObservableCollection<string>();
            _command = string.Empty;

            ConnectCommand = new BasicCommand((o) =>
            {
                _appDispatcher = Application.Current.RootVisual.Dispatcher;
                _commSocket = new TelnetSocket(ServerName, _port);
                _commSocket.MessageRecieved += new TelnetSocket.IncomingMessageHandler((m) => { AddContent(m); });
                _commSocket.Connect();
            });

            SendCommand = new BasicCommand((o) =>
                {
                    if (_commSocket.IsConnected)
                    {
                        _commSocket.Write(_command);
                    }
                });
        }

        void AddContent(string msg)
        {
            if (_appDispatcher == null || _appDispatcher.CheckAccess())
            {
                Content.Add(msg);
                FirePropertyChanged("Content");
            }
            else
            {
                _appDispatcher.BeginInvoke(() => { AddContent(msg); });
            }
        }

        void FirePropertyChanged(string propertyName)
        {
            if (_appDispatcher == null || _appDispatcher.CheckAccess())
            {
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            else
                _appDispatcher.BeginInvoke(() => { FirePropertyChanged(propertyName); });

        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
