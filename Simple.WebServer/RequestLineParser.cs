namespace Simple.WebServer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    public class RequestLineParser
    {
        private const int CR = '\r';
        private const int LF = '\n';

        public static RequestLine Parse(Stream stream)
        {
            int b = stream.ReadByte();
            while (b == CR || b == LF)
            {
                b = stream.ReadByte();
            }

            var bytes = new LinkedList<byte>();
            bytes.AddLast((byte) b);

            while (true)
            {
                b = stream.ReadByte();
                if (b == CR || b < 0)
                {
                    stream.ReadByte();
                    break;
                }
                bytes.AddLast((byte) b);
            }

            var text = Encoding.Default.GetString(bytes.ToArray());
            var parts = text.Split(' ');

            if (parts.Length == 3)
            {
                return new RequestLine(parts[0], parts[1], parts[2]);
            }

            throw new InvalidRequestException("Invalid Request Line.");
        }
    }

    [Serializable]
    public class InvalidRequestException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidRequestException()
        {
        }

        public InvalidRequestException(string message) : base(message)
        {
        }

        public InvalidRequestException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidRequestException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

    public class RequestLine
    {
        private readonly string _method;
        private readonly string _uri;
        private readonly string _httpVersion;

        public RequestLine(string method, string uri, string httpVersion)
        {
            _method = method;
            _uri = uri;
            _httpVersion = httpVersion;
        }

        public string HttpVersion
        {
            get { return _httpVersion; }
        }

        public string Uri
        {
            get { return _uri; }
        }

        public string Method
        {
            get { return _method; }
        }
    }
}