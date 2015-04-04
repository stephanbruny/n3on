module Server

open System
open System.IO
open System.Net
open System.Net.Sockets

open Http
open Utils

type Socket with
  member socket.AsyncAccept() = Async.FromBeginEnd(socket.BeginAccept, socket.EndAccept)

type ServerRequest ( client : TcpClient, message : byte[] ) =
    inherit Object()
    member this.Client with get() = client
    member this.Message with get() = message


let serverIpAddress address = IPAddress.Parse(address)
let serverEndPoint (ipAddress : IPAddress, port : int ) = new IPEndPoint(ipAddress, port)
let serverListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
let requestBufferSize = 1024

let rec readRequestStream (stream : NetworkStream, buffer : byte[], callback) = async {
    let! size = stream.AsyncRead(buffer)
    if size >= buffer.Length then
        let newBuffer : byte[] = Array.zeroCreate (buffer.Length + requestBufferSize)
        Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length)
        return! readRequestStream (stream, newBuffer, callback)
    else
        return callback buffer
}

let rec requestLoop (server : Socket, onRequest) = async {
    let! socket = server.AsyncAccept()
    let stream = new NetworkStream(socket, false)

    // let buffer = Array.zeroCreate 1024

    let requestCallback data = async {
        let reqStr = data
        let req = Http.deserialize reqStr
        let res : byte[] = onRequest req
        let! bytesSent = stream.AsyncWrite( res )
        stream.Close()
        socket.Shutdown(SocketShutdown.Both)
        socket.Close()
    }

    let buf : byte[] = Array.zeroCreate requestBufferSize
    // let! rq = requestCallback ( System.Text.Encoding.UTF8.GetString( Async.AwreadRequestStream ( stream, buf ) ) )
    Async.Start ( readRequestStream ( stream, buf, fun (data) -> 
        let req = System.Text.Encoding.UTF8.GetString(data)
        Async.Start(requestCallback req)
        )
    )

    return! requestLoop(server, onRequest)
}

let createServer ip port =
    let listener = serverListener 
    listener.Bind( serverEndPoint ( serverIpAddress ( ip ), port) )
    listener.Listen(int SocketOptionName.MaxConnections)
    listener

let runServer (server : Socket, onRequest) =
    Async.Start(requestLoop(server, onRequest))