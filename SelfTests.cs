using System.Net;
using System.Net.Sockets;
using System.Text;
namespace SSE;
static class SelfTests
{
    public static async Task Run()
    {
        var server = new HttpServer();
        Task running = server.Start("127.0.0.1", 13004);
        try
        {
            const string url = "http://127.0.0.1:13004/index.html";
            using var handler = new HttpClientHandler { CookieContainer = new CookieContainer(), UseProxy = false };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };
            for (int i = 1; i <= 3; i++)
            {
                using var response = await client.GetAsync(url);
                string body = await response.Content.ReadAsStringAsync();
                Check(response.StatusCode == HttpStatusCode.OK && body.Contains($"This is your {i}. visit"), $"visit {i}");
                Check(handler.CookieContainer.GetCookies(new Uri(url))["visits"]?.Value == i.ToString(), $"cookie {i}");
                Check(response.Headers.CacheControl?.NoStore == true, "no-store");
            }
            using var freshHandler = new HttpClientHandler { UseProxy = false };
            using var fresh = new HttpClient(freshHandler);
            Check((await fresh.GetStringAsync(url)).Contains("This is your 1. visit"), "separate visitor");
            using var missing = await client.GetAsync("http://127.0.0.1:13004/favicon.ico");
            Check(missing.StatusCode == HttpStatusCode.NotFound, "favicon does not count");
            using var post = await client.PostAsync(url, new StringContent(""));
            Check(post.StatusCode == HttpStatusCode.MethodNotAllowed, "POST rejected");
            using var rawHandler = new HttpClientHandler { UseCookies = false, UseProxy = false };
            using var rawClient = new HttpClient(rawHandler);
            foreach (string value in new[] { "bad", "-1", "9223372036854775807" })
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Cookie", "visits=" + value);
                using var response = await rawClient.SendAsync(request);
                Check((await response.Content.ReadAsStringAsync()).Contains("This is your 1. visit"), "invalid counter reset");
            }
            // A request sent in two writes exercises complete-header reading.
            using var tcp = new TcpClient();
            await tcp.ConnectAsync("127.0.0.1", 13004);
            using var stream = tcp.GetStream();
            await stream.WriteAsync(Encoding.ASCII.GetBytes("GET /index.html HTTP/1.1\r\nHost: localhost\r\nCoo"));
            await Task.Delay(25);
            await stream.WriteAsync(Encoding.ASCII.GetBytes("kie: other=x; visits=7\r\n\r\n"));
            using var reader = new StreamReader(stream);
            Check((await reader.ReadToEndAsync()).Contains("This is your 8. visit"), "fragmented request and multiple cookies");
            var parsed = new HttpMessage("HTTP/1.1 404 Not Found\r\nContent-Length: 0\r\n\r\n");
            Check(parsed.Method == null && parsed.StatusMessage == "Not Found", "response parsing");
            var unicode = new HttpMessage("200", "OK", null, "Grüße");
            string wire = unicode.ToString();
            Check(wire.Contains("Content-Length: 7\r\n"), "UTF-8 byte length");
            Console.WriteLine("ALL CHECKS PASSED");
        }
        finally { server.Stop(); }
        await running;
    }
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("FAIL: " + name);
        Console.WriteLine("PASS: " + name);
    }
}
