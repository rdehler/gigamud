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
    public enum TelnetOptions : byte
    {
        TransmitBinary = 0x0,
        Echo = 0x1,
        SupressGoAhead = 0x3,
        Status = 0x5,
        TimingMark = 0x6,
        NAOCRD = 0xA,
        NAOHTS = 0xB,
        NAOHTD = 0xC,
        NAOFFD = 0xD,
        NAOVTS = 0xE,
        NAOVTD = 0xF,
        NAOLFD = 0x10,
        ExtendedAscii = 0x11,
        TerminalType = 0x18,
        NAWS = 0x1F,
        TerminalSpeed = 0x20,
        ToggleFlowControl = 0x21,
        LienMode = 0x22,
        Authentication = 0x23
    }
}
