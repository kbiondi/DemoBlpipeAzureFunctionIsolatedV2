namespace Company.Function

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Azure.Functions.Worker
open Output

module HttpTrigger =
    
    [<Function("HttpTrigger")>]
    let run ([<HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)>]
        req: HttpRequestData) (executionContext:FunctionContext) =
        async {
            
            executionContext.GetLogger("HttpFunction")
                .LogInformation("F# HTTP trigger function processed a request.")

            return OkObjectResult(outputBlob.UploadAsync(blpipeOutput)) :> IActionResult
            
        } |> Async.StartAsTask