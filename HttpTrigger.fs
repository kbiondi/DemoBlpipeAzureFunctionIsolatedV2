namespace Company.Function

open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Newtonsoft.Json
open Microsoft.Extensions.Logging
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Azure.Functions.Worker
open Microsoft.AspNetCore.WebUtilities

module HttpTrigger =
    
    // Define a nullable container to deserialize into.
    [<AllowNullLiteral>]
    type NameContainer() =
        member val Name = "" with get, set

    // For convenience, it's better to have a central place for the literal.
    [<Literal>]
    let Name = "name"

    [<Function("HttpTrigger")>]
    let run ([<HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)>]
        req: HttpRequestData) (executionContext:FunctionContext) =
        async {
            
            executionContext.GetLogger("HttpFunction")
                .LogInformation("F# HTTP trigger function processed a request.")

            let queryDictionary = 
                QueryHelpers.ParseQuery(req.Url.Query)

            let nameOpt = 
                if req.Url.Query.Contains(Name) then
                    Some(queryDictionary[Name].Item(0))
                else
                    None

            use stream = new StreamReader(req.Body)
            let! reqBody = stream.ReadToEndAsync() |> Async.AwaitTask

            let data = JsonConvert.DeserializeObject<NameContainer>(reqBody)

            let name =
                    match nameOpt with
                    | Some n -> n
                    | None -> 
                            match data with
                            | null -> ""
                            | nc -> nc.Name
            
            let responseMessage =             
                if (String.IsNullOrWhiteSpace(name)) then
                    "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                else
                    "Hello, " + name + ". This HTTP triggered function executed successfully."

            return OkObjectResult(responseMessage) :> IActionResult
        } |> Async.StartAsTask