namespace Simple.WebServer.Owin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public delegate Task AppDelegate(
        IDictionary<string, object> env,
        Stream requestBody,
        ResultDelegate result,
        Action<Exception> fault);

    public delegate Task ResultDelegate(
        string status,
        IDictionary<string, IEnumerable<string>> headers,
        BodyDelegate body);

    public delegate Task BodyDelegate(
        Stream stream,
        CancellationToken cancellationToken);
}