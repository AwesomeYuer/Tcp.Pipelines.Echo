using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Socket clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

Console.WriteLine("Connecting to port 8087");

clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, 8087));
await using var stream = new NetworkStream(clientSocket);

// like telnet client mode

// run async
_ = OnReadProcessAsync();

using var standardInput = Console.OpenStandardInput();

// run sync
await standardInput.CopyToAsync(stream);

async Task OnReadProcessAsync()
{
    using var standardOutput = Console.OpenStandardOutput();
    await stream!.CopyToAsync(standardOutput);
}
