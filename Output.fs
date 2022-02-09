module Output

open System.IO
open Input
open FSharp.Json
open Bloomberglp.Blpapi.Examples.RefDataExample

let result2 = Json.serialize result
let outputBlob = containerClient.GetBlobClient("/blpipe/BlpipeOutput.json")
let blpipeOutput = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(result2))