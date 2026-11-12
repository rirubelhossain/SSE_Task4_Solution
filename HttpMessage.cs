using System.Text;
namespace SSE;

// Text messages for this HTTP/1.1 teaching exercise.
public class HttpMessage
{
    public const string GET = "GET", POST = "POST", PUT = "PUT", DELETE = "DELETE",
        HEAD = "HEAD", OPTIONS = "OPTIONS", TRACE = "TRACE";
    public string? Method, Host, Resource, StatusCode, StatusMessage;
    public Dictionary<string, string> Headers = new(StringComparer.OrdinalIgnoreCase);
    public string Content = "";

    public HttpMessage(string method, string host, string resource,
        Dictionary<string, string>? headers, string content)
    {
        Method = method; Host = host; Resource = resource; Content = content;
        if (headers != null) Headers = new(headers, StringComparer.OrdinalIgnoreCase);
    }
    public HttpMessage(string statusCode, string statusMessage,
        Dictionary<string, string>? headers, string content)
    {
        StatusCode = statusCode; StatusMessage = statusMessage; Content = content;
        if (headers != null) Headers = new(headers, StringComparer.OrdinalIgnoreCase);
    }
    public HttpMessage(string message)
    {
        int split = message.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        if (split < 0) throw new FormatException("Missing HTTP header terminator");
        Content = message[(split + 4)..];
        string[] lines = message[..split].Split("\r\n");
        string[] first = lines[0].Split(' ', 3);
        if (first.Length != 3) throw new FormatException("Malformed start line");
        if (first[0] is "HTTP/1.1" or "HTTP/1.0")
        { StatusCode = first[1]; StatusMessage = first[2]; }
        else
        {
            if (first[2] is not ("HTTP/1.1" or "HTTP/1.0"))
                throw new FormatException("Unsupported HTTP version");
            Method = first[0]; Resource = first[1];
        }
        foreach (string line in lines.Skip(1))
        {
            int colon = line.IndexOf(':');
            if (colon <= 0) throw new FormatException("Malformed header");
            string name = line[..colon].Trim(), value = line[(colon + 1)..].Trim();
            if (name.Equals("Host", StringComparison.OrdinalIgnoreCase)) Host = value;
            else if (name.Equals("Cookie", StringComparison.OrdinalIgnoreCase) && Headers.ContainsKey(name))
                Headers[name] += "; " + value;
            else Headers[name] = value;
        }
    }
    public override string ToString()
    {
        Headers["Content-Length"] = Encoding.UTF8.GetByteCount(Content).ToString();
        var result = new StringBuilder();
        if (Method != null)
            result.Append($"{Method} {Resource} HTTP/1.1\r\nHost: {Host}\r\n");
        else result.Append($"HTTP/1.1 {StatusCode} {StatusMessage}\r\n");
        foreach (var header in Headers)
            result.Append($"{header.Key}: {header.Value}\r\n");
        return result.Append("\r\n").Append(Content).ToString();
    }
    public void SetCookie(string name, string value)
    {
        if (string.IsNullOrEmpty(name) || name.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '_' && c != '-'))
            throw new ArgumentException("Invalid cookie name");
        string pair = name + "=" + Uri.EscapeDataString(value);
        if (Method == null)
        {
            // One response cookie is sufficient for this assignment.
            Headers["Set-Cookie"] = pair + "; Path=/; HttpOnly; SameSite=Lax";
        }
        else
        {
            var cookies = GetCookies();
            cookies[name] = value;
            Headers["Cookie"] = string.Join("; ", cookies.Select(c => c.Key + "=" + Uri.EscapeDataString(c.Value)));
        }
    }
    public Dictionary<string, string> GetCookies()
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        string headerName = Method == null ? "Set-Cookie" : "Cookie";
        if (!Headers.TryGetValue(headerName, out string? header)) return result;
        // Response attributes (Path, HttpOnly, SameSite) are not cookies.
        var pairs = Method == null ? new[] { header.Split(';')[0] } : header.Split(';');
        foreach (string part in pairs)
        {
            int equals = part.IndexOf('=');
            if (equals <= 0) continue;
            string name = part[..equals].Trim(), value = part[(equals + 1)..].Trim();
            result[name] = Uri.UnescapeDataString(value);
        }
        return result;
    }
}
