module Response

open System.Collections.Generic
open Http

type response (encoding : System.Text.Encoding) = 
    let Header = new Dictionary<string, string>()

    let mutable StatusCode = "200"
    let mutable StatusMessage = "OK"
    let mutable TextEncoding = encoding

    member this.Status (code: string, message: string) =
        StatusCode <- code
        StatusMessage <- message
        this
    member this.Encoding (enc : System.Text.Encoding) =
        TextEncoding <- enc
        this
    
    member this.SetHeader(key : string, value : string) =
        Header.[key] <- value

    member this.Send (content : string) =
        Header.["Content-Length"] <- content.Length.ToString()
        let header = Http.createHttpHeader StatusCode StatusMessage
        let headerData = Http.serializeHeader Header
        let responseString = header + headerData + "\r\n" + content
        System.Console.WriteLine( responseString )
        TextEncoding.GetBytes(responseString)
