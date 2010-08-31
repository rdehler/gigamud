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

namespace MMX.Communications.Sockets.Telnet
{
    public enum TelnetCommands : byte
    {
        SubnegotiationEnd = 0xF0,
        NoOp = 0xF1,
        DataMark = 0xF2,
        Break = 0xF3,
        InterruptProcess = 0xF4,
        AbortOutput = 0xF5,
        AreYouThere = 0xF6,
        EraseCharacter = 0xF7,
        EraseLine = 0xF8,
        GoAhead = 0xF9,
        Subnegotation = 0xFA,
        Will = 0xFB,
        Wont = 0xFC,
        Do = 0xFD,
        Dont = 0xFE,
        IAC = 0xFF
    }

    public static class TelnetCommandExtensions
    {
        public static byte Value(TelnetCommands command) { return (byte)command;}
    }
}
