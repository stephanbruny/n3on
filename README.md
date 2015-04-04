# N3ON - Webserver #

A simple web server utilizing asynchronous and functional methodologies written in F#.
Using the Mono-(.NET)-Framework to allow platform-independence.

## Buidling ##

- 1. Get the latest Version of Xamarin Studio (www.xamarin.com/studio) (''should also compile with Visual Studio 13'')
- 2. Download the source
- 3. Compile and run

## How to use ##

See *Program.fs*

```
open System
open System.Threading
open System.Collections.Generic
open Server
open Http
open Response
open Middleware
open TcpServer

let Version = "0.0.0.1"

let defaultAction req =
    let res = new response(Text.Encoding.UTF8);
    Middleware.exec req res

[<EntryPoint>]
let main argv = 
    printfn "n3on Webserver"
    printfn "Version: %s" Version

    let server = new TcpServer.Server("127.0.0.1", 1337, defaultAction)

    let myFirstAction (_req: request,_res: response, _next) =
        _res.SetHeader("Server", "N3ON .Net Server")
        _next()

    let myAction (_req: request,_res: response, _next) = 
        let reqHeader = Http.serializeHeader (_req.header)
        let content = Http.getContent _req
        match _req.url, _req.httpMethod with
            | "/", "GET" -> _res.Status("200", "OK").Send("index")
            | "/test", "GET" -> _res.Status("200", "OK").Send("test")
            | "/", "POST" -> _res.Status("200", "OK").Send("Nothing to POST here")
            | _ -> _res.Status("404", "Not found").Send("404 - Not found")
   
    Middleware.add(myFirstAction)
    Middleware.add(myAction)

    server.WaitForConnection

    while true do
        Thread.Sleep(1000)
    0 // return an integer exit code
```
