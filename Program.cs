namespace SSE;
class Program
{
    static async Task Main(string[] args)
    {
        if (args.Contains("--test")) { await SelfTests.Run(); return; }
        var server = new HttpServer();
        Console.WriteLine("Open your browser: http://127.0.0.1:3000/index.html");
        Console.WriteLine("Refresh to increase the visit counter. Stop with Ctrl+C.");
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; server.Stop(); };
        await server.Start("127.0.0.1", 3000);
    }
}
