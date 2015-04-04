module TcpServer
open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading

open Http
open Response

type ServerRequest = { Client : TcpClient; Message : byte[] }

type Server(address, port, onRequest : request -> byte[]) =
    let Address   = IPAddress.Parse(address);
    let EndPoint  = new IPEndPoint (Address, port);
    let Listener  = new TcpListener (EndPoint);
    do Listener.Start ();

    member this.Close =
        Listener.Stop()

    member this.ProcessClient (result : IAsyncResult) =
        let client = Listener.EndAcceptTcpClient(result);
        let clientStream = client.GetStream ();
        this.WaitForRequest(client, clientStream, new AsyncCallback(this.RequestCallback));
        this.WaitForConnection

    member this.WaitForConnection =
        ignore(Listener.BeginAcceptTcpClient (new AsyncCallback (this.ProcessClient), new System.Object()))
        ()

    member this.WaitForRequest(tcpClient : TcpClient, stream : NetworkStream, callback : AsyncCallback) =
        let buffer : byte[] = Array.zeroCreate(tcpClient.ReceiveBufferSize)
        ignore(stream.BeginRead (buffer, 0, buffer.Length, callback, { Client = tcpClient; Message = buffer }));
        ()

    member this.RequestCallback(asyncResult : IAsyncResult) =
        let req = asyncResult.AsyncState :?> ServerRequest
        let stream = req.Client.GetStream ();
        let read = stream.EndRead (asyncResult);
        if read = 0 then
            stream.Close ();
            req.Client.Close ();
        else
            let data = System.Text.Encoding.UTF8.GetString(req.Message, 0, read)
            let requestHeader = Http.deserialize data
            let response = new response (System.Text.Encoding.UTF8)
            let requestHandler = async {
                let res = onRequest requestHeader
                stream.Write(res, 0, res.Length)
            }
            Async.Start(requestHandler)
            ignore( this.WaitForRequest (req.Client, req.Client.GetStream(), new AsyncCallback (this.RequestCallback)) );
            ()