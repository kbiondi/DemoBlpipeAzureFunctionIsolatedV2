module Input

open System
open Azure.Storage.Blobs
open FSharp.Data

let connection = Environment.GetEnvironmentVariable("ConnectionStrings__azu004stapd004", EnvironmentVariableTarget.Process)
let blobServiceClient = BlobServiceClient(connection)
let blobContainer = "landing"
let containerClient = blobServiceClient.GetBlobContainerClient(blobContainer)
let blobClient = containerClient.GetBlobClient("/blpipe/CusipsMnemonics.json")
let input = blobClient.Download().Value.Content

type JsonTypeProvider = JsonProvider<"""{"cusips":["912810SX Govt"],"mnemonics":["NAME"]}""">
let parameters = JsonTypeProvider.Load(input)