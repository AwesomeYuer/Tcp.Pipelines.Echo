using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

var clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

Console.WriteLine("Connecting to port 8087");

clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, 8087));
var stream = new NetworkStream(clientSocket);

// run async
_ = OnReadProcessAsync();

// run sync
await Console.OpenStandardInput().CopyToAsync(stream);

async Task OnReadProcessAsync()
{
    var standardOutput = Console.OpenStandardOutput();
    await stream!.CopyToAsync(standardOutput);
}
