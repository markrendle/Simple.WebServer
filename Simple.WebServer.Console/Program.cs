using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.WebServer.Owin;

namespace Simple.WebServer.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var application = new Application();
            var server = new Server(application.App, 3000);

            server.Start();

            System.Console.WriteLine("Running on port 3000");
            System.Console.Write("Press Enter to quit: ");
            System.Console.ReadLine();
            server.Stop();
        }
    }

    class Application
    {
        private static readonly byte[] Output = Encoding.Default.GetBytes(Properties.Resources.HelloWorldHtml);
        public Task App(IDictionary<string, object> env, Stream body, ResultDelegate result, Action<Exception> fault)
        {
            var headers = new Dictionary<string, IEnumerable<string>>
                              {
                                  {"Content-Type", new[] {"text/html"}},
                                  {"Content-Length", new[] {Output.Length.ToString(CultureInfo.InvariantCulture)}}
                              };
            return result("200 OK", headers, (stream, token) => stream.WriteAsync(Output, 0, Output.Length));
        }
    }
}
