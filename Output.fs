module Output

open System.IO
open Input
open FSharp.Json
open Bloomberglp.Blpapi.Examples.RefDataExample

// ReferenceLoopHandling.Ignore |> ignore
// let pme2 = JsonConvert.SerializeObject(delta, JsonSerializerSettings(ReferenceLoopHandling = ReferenceLoopHandling.Ignore))

// let pme2 = Json.serialize (Seq.toList delta)
let result2 = Json.serialize result
// let pme2 = JsonConvert.SerializeObject(delta)
let outputBlob = containerClient.GetBlobClient("/blpipe/BlpipeOutput.json")
let blpipeOutput = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(result2))
// pmeOutput.Position <- (int64) 0