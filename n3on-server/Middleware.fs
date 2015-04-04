module Middleware

open System
open System.Collections.Generic
open System.Text.RegularExpressions

open Response
open Http

let middlewares = new List<request * response * (unit -> byte[]) -> byte[] >()

let encoding = System.Text.Encoding.UTF8

let add action =
    middlewares.Add(action)
    ()

let rec next (req : request, res : response, index : int) =
    try
        let action = middlewares.[index]
        let nextIndex = index + 1
        action (req, res, fun () -> next (req, res, nextIndex) )
    with
        | _ -> [||]

let rec exec req res =
    next (req, res, 0)