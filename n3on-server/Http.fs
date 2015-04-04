module Http

open System
open System.Net
open System.IO
open System.Collections.Generic

open System.Text.RegularExpressions

type request = {
    raw : string;
    http : string;
    httpMethod : string;
    url : string;
    header : Dictionary<string, string>
    content : string
}

let getHeader req key =
    if req.header.ContainsKey(key) then
        req.header.[key]
    else
        null

let setHeader req key value force =
    if req.header.ContainsKey(key) then
        if force then
            req.header.[key] <- value
            true
        else
            false
    else
        req.header.[key] <- value
        true

let httpRegexp = @"\S+";
let topHeaderRegexp = @"^HTTP"
let headerKeyRegexp = @"^(\S+)(?=\:)"
let headerValRegexp = @"\:\s(\S+)$"

let endl = "\r\n"

let createHttpHeader statusCode message =
    "HTTP/1.1 " + statusCode + " " + message + endl;

let headerLine k v = k + ": " + v + endl

let serializeHeader (header : Dictionary<string, string>) : string =
    let map = header |> Seq.map( fun (kv) -> headerLine kv.Key kv.Value )
    String.Concat(map)

let defaultRequest = 
    {
        httpMethod = "Unknown";
        url = "";
        http = "";
        header = new Dictionary<string, string>()
        content = ""
        raw = ""
    }

let getContent (req : request) =
    try 
        let length = System.Int32.Parse( req.header.["Content-Length"] )
        let pos = req.raw.Length - length
        let result = req.raw.Substring(pos, length)
        result
    with
        | _ -> ""

let deserializeHttp httpData rawData =
    try
        let matches = Regex.Matches(httpData, httpRegexp)
        {
            httpMethod = matches.[0].Value; 
            url = matches.[1].Value; 
            http = matches.[2].Value; 
            header = new Dictionary<string, string>()
            content = ""
            raw = rawData
        }
    with
        | :? System.IndexOutOfRangeException as ex ->
            printfn "Could not parse header: %s" ex.Message
            defaultRequest
        | _ ->
            printfn "Could not create request type"
            defaultRequest

let deserialize data = 
    use sr = new StringReader(data)
    let firstLine = sr.ReadLine()
    let raw = data
    let result = deserializeHttp firstLine raw
    let mutable line = sr.ReadLine()
    while true <> String.IsNullOrEmpty(line) do
        result.header.Add(Regex.Match(line, headerKeyRegexp).Value, Regex.Match(line, headerValRegexp).Groups.[1].Value)
        line <- sr.ReadLine()
    result
