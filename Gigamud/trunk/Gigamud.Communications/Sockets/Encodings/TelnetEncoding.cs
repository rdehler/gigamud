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
using System.Text;
using System.Collections.Generic;

namespace Gigamud.Communications.Sockets.Encodings
{
    public static class TelnetEncoding
    {
        public static HashSet<byte> _invalidBytes;

        static TelnetEncoding()
        {
            // build valid bytes
            _invalidBytes = new HashSet<byte>();

            foreach (char c in new char[] { '\b', '\r' })
                _invalidBytes.Add((byte)c);
        }

        public static string ConvertFromBytes(byte[] source, int index, int length)
        {
            StringBuilder sb = new StringBuilder();

            int len = source.Length;

            for (int i = 0; i < length; ++i)
            {
                // read to end of stream check
                if (index + i > len) break;

                byte b = source[index + i];

                if ((b ^ 0xFF) > 0) // normal processing
                {
                    if (!_invalidBytes.Contains(b))
                        sb.Append((char)b);
                }
                else
                {
                    i += 2;
                    // process special commands
                }
            }

            return sb.ToString();
        }
    }
}
