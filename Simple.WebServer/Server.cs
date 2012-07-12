using System.IO;

namespace Simple.WebServer
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Owin;

    public class Server
    {
        private readonly AppDelegate _app;
        private static readonly IPAddress Localhost = new IPAddress(new byte[] {0,0,0,0});
        private readonly TcpListener _listener;
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        private int _started = 0;

        public Server(AppDelegate app, int port) : this(app, Localhost, port)
        {
        }

        public Server(AppDelegate app, IPAddress ipAddress, int port)
        {
            _app = app;
            _ipAddress = ipAddress;
            _port = port;
            _listener = new TcpListener(_ipAddress, _port);
        }

        public void Start()
        {
            if (0 != Interlocked.CompareExchange(ref _started, 1, 0)) throw new InvalidOperationException("Server is already started.");

            _listener.Start();
            _listener.BeginAcceptTcpClient(Callback, null);
        }

        public void Stop()
        {
            _listener.Stop();
        }

        private async void Callback(IAsyncResult ar)
        {
            TcpClient client;
            try
            {
                client = _listener.EndAcceptTcpClient(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            _listener.BeginAcceptTcpClient(Callback, null);
            var instance = new Instance(client, _app);
            await instance.Run();
        }
    }

    internal class Instance
    {
        private static readonly byte[] CRLF = Encoding.UTF8.GetBytes("\r\n");

        private readonly TcpClient _client;
        private readonly AppDelegate _app;

        public Instance(TcpClient client, AppDelegate app)
        {
            _client = client;
            _app = app;
        }

        public Task Run()
        {
            var env = new Dictionary<string, object>
                {
                    { OwinConstants.Version, "0.8" }
                };
            var networkStream = _client.GetStream();
            try
            {

            }
            catch (IOException)
            {
                
            }
            var requestLine = RequestLineParser.Parse(networkStream);
            env[OwinConstants.RequestMethod] = requestLine.Method;
            if (requestLine.Uri.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                Uri uri;
                if (Uri.TryCreate(requestLine.Uri, UriKind.Absolute, out uri))
                {
                    env[OwinConstants.RequestPath] = uri.AbsolutePath;
                    env[OwinConstants.RequestQueryString] = uri.Query;
                    env[OwinConstants.RequestScheme] = uri.Scheme;
                }
            }
            return _app(env, networkStream, Result, ex => { });
        }

        private async Task Result(string status, IDictionary<string, IEnumerable<string>> headers, BodyDelegate body)
        {
            var stream = _client.GetStream();
            var bytes = Encoding.UTF8.GetBytes("HTTP/1.1 " + status + "\r\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);

            foreach (var header in headers)
            {
                foreach (var value in header.Value)
                {
                    bytes = Encoding.UTF8.GetBytes(header.Key + ": " + value + "\r\n");
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                }
            }
            await stream.WriteAsync(CRLF, 0, 2);
            await body(stream, CancellationToken.None);
        }
    }
}
