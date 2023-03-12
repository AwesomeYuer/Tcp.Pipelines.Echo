using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
        
var listenerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
listenerSocket.Bind(new IPEndPoint(IPAddress.Any, 8087));

Console.WriteLine("Listening on port 8087");

listenerSocket.Listen(120);

while (true)
{
    // run sync
    var socket = await listenerSocket.AcceptAsync();
    // run async
    _ = ProcessLinesAsync(socket);
}
        
static async Task ProcessLinesAsync(Socket socket)
{
    Console.WriteLine($"[{socket.RemoteEndPoint}]: connected");

    // Create a PipeReader over the network stream
    NetworkStream networkStream = new NetworkStream(socket);
    PipeReader pipeReader = PipeReader.Create(networkStream);

    while (true)
    {
        ReadResult result = await pipeReader.ReadAsync();
        ReadOnlySequence<byte> buffer = result.Buffer;

        while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
        {
            // Process the line.
            ProcessLine(line);

            var l = line.Length;

            await networkStream
                        .WriteAsync
                                (
                                    Encoding
                                            .UTF8
                                            .GetBytes("server received line: [")
                                            .Concat
                                                (
                                                    line.Slice(0, l - 1).ToArray()
                                                )
                                            .Concat
                                                (
                                                    new byte[] { (byte) ']', (byte) '\n' }
                                                )
                                            .ToArray()
                                );
            await networkStream
                            .FlushAsync();

        }

        // Tell the PipeReader how much of the buffer has been consumed.
        pipeReader.AdvanceTo(buffer.Start, buffer.End);

        // Stop reading if there's no more data coming.
        if (result.IsCompleted)
        {
            break;
        }
    }

    // Mark the PipeReader as complete.
    await pipeReader.CompleteAsync();

    Console.WriteLine($"[{socket.RemoteEndPoint}]: disconnected");
}

static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
{
    // Look for a EOL in the buffer.
    SequencePosition? position = buffer.PositionOf((byte) '\n');

    if (position == null)
    {
        line = default;
        return false;
    }

    // Skip the line + the \n.
    line = buffer.Slice(0, position.Value);
    buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
    return true;
}

static void ProcessLine(in ReadOnlySequence<byte> buffer)
{
    foreach (var segment in buffer)
    {
        Console.Write(Encoding.UTF8.GetString(segment.Span));
    }
    Console.WriteLine();
}
