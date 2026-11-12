using System.Net;
using System.Net.Sockets;
using System.Text;
namespace SSE;
public class TcpServer
{
    private CancellationTokenSource? _ts;
    private Task _serverTask = Task.CompletedTask;
    protected virtual string HandleRequest(string msg) => "";
    public Task Start(string ip, int port)
    {
        _ts = new CancellationTokenSource();
        _serverTask = Listen(ip, port, _ts.Token);
        return _serverTask;
    }
    public void Stop() { _ts?.Cancel(); _serverTask.GetAwaiter().GetResult(); }
    private async Task Listen(string ip, int port, CancellationToken token)
    {
        var listener = new TcpListener(IPAddress.Parse(ip), port);
        listener.Start();
        try
        {
            while (!token.IsCancellationRequested)
            {
                using var client = await listener.AcceptTcpClientAsync(token);
                using var stream = client.GetStream();
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
                timeout.CancelAfter(TimeSpan.FromSeconds(10));
                try
                {
                    // Read through the complete header terminator, even when TCP fragments it.
                    var bytes = new List<byte>();
                    var one = new byte[1];
                    while (true)
                    {
                        int read = await stream.ReadAsync(one.AsMemory(), timeout.Token);
                        if (read == 0) break;
                        bytes.Add(one[0]);
                        int n = bytes.Count;
                        if (n > 32768) throw new IOException("Headers too large");
                        if (n >= 4 && bytes[n-4] == 13 && bytes[n-3] == 10 && bytes[n-2] == 13 && bytes[n-1] == 10) break;
                    }
                    if (bytes.Count == 0) continue;
                    string request = Encoding.UTF8.GetString(bytes.ToArray());
                    byte[] response = Encoding.UTF8.GetBytes(HandleRequest(request));
                    await stream.WriteAsync(response.AsMemory(), timeout.Token);
                }
                catch (OperationCanceledException) when (!token.IsCancellationRequested) { }
                catch (IOException ex) { Console.WriteLine("Connection closed: " + ex.Message); }
                catch (SocketException ex) { Console.WriteLine("Connection closed: " + ex.Message); }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        finally { listener.Stop(); }
    }
}
