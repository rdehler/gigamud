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
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace MMX.Infrastructure.Formatter
{
    public static class TextFormatter
    {
        static Regex _combatRx, _dmgRx;
        static Regex _failRx;
        static Regex _missRx;
        static Regex _opMissRx;

        static RegexOptions SearchOptions = RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.IgnoreCase;

        static TextFormatter()
        {
            _combatRx = new Regex("^You.*damage[.|!]$", SearchOptions);
            _dmgRx = new Regex(".+you.*for.*damage[.|!]$", SearchOptions);
        }

        public static Run[] Format(string s)
        {
            List<Run> runs = new List<Run>();
            string[] data = s.Split(':');
            for (int i = 0; i < data.Length; ++i)
            {
                string str = data[i];
                if (i < data.Length - 1) str += ":";
                Run r = new Run();
                r.Text = str;
                r.Foreground = new SolidColorBrush(Colorize(str));
                runs.Add(r);
            }
            return runs.ToArray();
        }

        static Color Colorize(string s)
        {
            if (_combatRx.IsMatch(s))
                return Colors.Red;
            if (_dmgRx.IsMatch(s))
                return Colors.Orange;
            return Colors.Gray;
        }
    }
}
