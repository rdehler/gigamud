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

namespace Gigamud.Infrastructure.Formatter
{
    public static class TextFormatter
    {
        public static Run Format(string s)
        {
            Run r = new Run();
            r.Text = s;
            r.Foreground = new SolidColorBrush(Colorize(s));
            return r;
        }

        static Color Colorize(string s)
        {
            if (s.Contains("say"))
                return Colors.Green;
            if (s.Contains("damage"))
                return Colors.Red;
            if (s.Contains("experience"))
                return Colors.Yellow;
            return Colors.Gray;
        }
    }
}
