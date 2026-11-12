using System.Globalization;
namespace SSE;
public class HttpServer : TcpServer
{
    protected override string HandleRequest(string msg)
    {
        try { return ReceiveRequest(new HttpMessage(msg)).ToString(); }
        catch (FormatException)
        { return Reply("400", "Bad Request", "<p>Malformed request.</p>").ToString(); }
    }
    protected virtual HttpMessage ReceiveRequest(HttpMessage request)
    {
        // Query strings do not change which page is requested.
        string path = (request.Resource ?? "").Split('?', 2)[0];
        if (path != "/" && path != "/index.html")
            return Reply("404", "Not Found", "<p>The requested file could not be found.</p>");
        if (request.Method != HttpMessage.GET)
        {
            var rejected = Reply("405", "Method Not Allowed", "<p>Use GET for this exercise.</p>");
            rejected.Headers["Allow"] = "GET";
            return rejected;
        }
        var cookies = request.GetCookies();
        long previous = 0;
        if (cookies.TryGetValue("visits", out string? value) &&
            long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out long parsed) &&
            parsed >= 0 && parsed < long.MaxValue)
            previous = parsed;
        long current = previous + 1;
        var response = Reply("200", "OK",
            "<!doctype html><html><head><meta charset=\"utf-8\"><title>SSE Visit Counter</title></head>" +
            $"<body><h1>This is your {current}. visit</h1><p>Number of prior visits: {previous}</p>" +
            "<p>Refresh this page to increase the counter.</p></body></html>");
        response.SetCookie("visits", current.ToString(CultureInfo.InvariantCulture));
        Console.WriteLine($"GET {path}: previous={previous}, current={current}");
        return response;
    }
    private static HttpMessage Reply(string code, string reason, string body)
        => new(code, reason, new Dictionary<string, string>
        {
            ["Content-Type"] = "text/html; charset=utf-8",
            ["Cache-Control"] = "no-store",
            ["Connection"] = "close"
        }, body);
}
