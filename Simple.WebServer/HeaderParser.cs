using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.WebServer
{
    using System.Collections.Specialized;
    using System.IO;

    public class HeaderParser
    {
        private const int CR = '\r';
        private const int LF = '\n';

        public static IDictionary<string,IEnumerable<string>> Parse(Stream stream)
        {
            var headers = new NameValueCollection();
            var bytes = ReadHeaderBytes(stream);

            var text = Encoding.Default.GetString(bytes);
            var lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (line == "")
                {
                    break;
                }
                var colonIndex = line.IndexOf(':');
                headers.Add(line.Substring(0, colonIndex), line.Substring(colonIndex + 1).TrimStart());
            }

            var dict = new Dictionary<string, IEnumerable<string>>(headers.Keys.Count);
            foreach (var key in headers.Keys.Cast<string>())
            {
                dict.Add(key, headers.GetValues(key));
            }

            return dict;
        }

        private static byte[] ReadHeaderBytes(Stream stream)
        {
            var bytes = new LinkedList<byte>();
            bool mightBeEnd = false;

            while (true)
            {
                int b = stream.ReadByte();
                if (b == CR)
                {
                    b = stream.ReadByte();
                    if (b != LF)
                    {
                        bytes.AddLast(CR);
                        bytes.AddLast((byte) b);
                        continue;
                    }
                    if (mightBeEnd)
                    {
                        break;
                    }
                    mightBeEnd = true;
                    bytes.AddLast(CR);
                    bytes.AddLast(LF);
                    continue;
                }
                mightBeEnd = false;
                bytes.AddLast((byte) b);
            }
            return bytes.ToArray();
        }
    }
}
