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

        StringBuilder _feed;
        public string DataFeed
        {
            get
            {
                return _feed.ToString();
            }
            set
            {
                _feed = new StringBuilder(value);
                FirePropertyChanged("DataFeed");
            }
        }

        public ICommand ConnectCommand { get; set; }

        TelnetSocket _commSocket;
        Dispatcher _appDispatcher;

        public TelnetConsoleViewModel()
        {
            _serverName = "time-a.nist.gov";
            _port = 13;
            _feed = new StringBuilder();

            ConnectCommand = new BasicCommand((o) =>
            {
                _appDispatcher = Application.Current.RootVisual.Dispatcher;
                _commSocket = new TelnetSocket(ServerName, _port);
                _commSocket.Connected += new TelnetSocket.TelnetSocketConnectedHandler((e) => {/*handle error*/});
                _commSocket.MessageRecieved += new TelnetSocket.TelnetSocketMessageRecievedHandler((m) => { _feed.AppendLine(m); FirePropertyChanged("DataFeed"); });
                _commSocket.Connect();
            });
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
