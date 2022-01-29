namespace Bloomberglp.Blpapi.Examples

open Bloomberglp.Blpapi
open System.Diagnostics
open System
open FSharp.Json
open Schemas

module RefDataExample =

  //API Parameters
  let requestService = "//blp/refdata"
  let RESPONSE_ERROR = Name("responseError")
  let CATEGORY = Name.GetName("category")
  let MESSAGE = Name("message")
  let SECURITY_DATA = Name.GetName("securityData")
  let SECURITY = Name.GetName("security")
  let SECURITY_ERROR = Name.GetName("securityError")
  let FIELD_DATA = Name.GetName("fieldData")
  let FIELD_EXCEPTIONS = Name.GetName("fieldExceptions")
  let FIELD_ID = Name.GetName("fieldId")
  let ERROR_INFO = Name.GetName("errorInfo")
  let requestType = "ReferenceDataRequest"
  let responseType = "ReferenceDataResponse"
  let serverHost = Environment.GetEnvironmentVariable("BLOOMBERG_SERVER_HOST", EnvironmentVariableTarget.User)
  let appName = Environment.GetEnvironmentVariable("BLOOMBERG_APP_NAME", EnvironmentVariableTarget.User)
  let serverPort = Int32.Parse (Environment.GetEnvironmentVariable("BLOOMBERG_SERVER_PORT", EnvironmentVariableTarget.User))
    
  let setOptions host port name =
    let options = SessionOptions()
    options.ServerHost <- host
    options.ServerPort <- port
    options.AuthenticationOptions <- "AuthenticationMode=APPLICATION_ONLY;ApplicationAuthenticationType=APPNAME_AND_KEY;ApplicationName=CALPERS:Workspace"
    let authOptions = AuthOptions(AuthApplication(name))
    let dAuthCorrelationId = CorrelationID("authCorrelation")
    options.SetSessionIdentityOptions(authOptions, dAuthCorrelationId) |> ignore
    printfn $"Session Options: {options}"
    options

  let processMiscEvents (eventObj:Event) =
      printfn $"Processing {eventObj.Type}"
      for msg:Message in eventObj do
        printfn "%s : %s\n" (System.DateTime.Now.ToString("s")) (msg.MessageType.ToString())
      seq {{ RequestId = None; ResponseError = None; SecurityData = None }}

  let processAdminEvent (eventObj:Event) =
    printfn "Processing ADMIN"
    for msg:Message in eventObj do
      match msg.MessageType.ToString() with
      | "SlowConsumerWarning" -> printfn "%s" (msg.ToString())
      | "SlowConsumerWarningCleared" -> printfn "%s" (msg.ToString())
      | _ -> printfn "%s %s" (System.DateTime.Now.ToString("s")) (msg.ToString())
    seq {{ RequestId = None; ResponseError = None; SecurityData = None }}

  let processAuthEvent (eventObj:Event) (session:Session) =
    printfn $"Processing Authorization STATUS: {eventObj.Type.ToString()}"
    for msg:Message in eventObj.GetMessages() do
      match msg.MessageType.ToString() with
      | "AuthorizationSuccess" -> printfn "%s: Authorization Success" (DateTime.Now.ToString("s"))
                                  printfn "%s" (msg.ToString())

      | "AuthorizationFailure" -> printfn "%s: Authorization Failure" (DateTime.Now.ToString("s"))
                                  printfn "%s" (msg.ToString())

      | "AuthorizationRevoked" -> printfn "%s: Authorization Revoked" (DateTime.Now.ToString("s"))
                                  printfn "%s" (msg.ToString())
      | _ -> printfn "%s: %s Authorization Revoked" (DateTime.Now.ToString("s")) (msg.MessageType.ToString())
             printfn "%s" (msg.ToString())
    printfn ""
    seq {{ RequestId = None; ResponseError = None; SecurityData = None }}            

  let createSession options =
    printfn "Creating session"
    new Session(options)
    
  let createRequest (session:Session) service =
    let service = session.GetService(service)
    let request = service.CreateRequest("ReferenceDataRequest")
    request.Set("returnEids", true)
    request

  let createSecuritiesRequestList (request:Request) =
    let securities = request["securities"]
    let fields = request["fields"]
    let cusips = ["912810SX Govt"; "912810SZ Govt"; "912810TA Govt"; "91282CCS Govt"; "SHAZAM"; "Hello"]
    let mnemonics = ["NAME"; "MTY_YEARS"; "CRNCY"; "BVAL_CONVERTIBLE_OPTION_VALUE"; "BLAH_BLAH_BLAH"; "F# is awesome!"]
    List.iter (fun x -> securities.AppendValue(x:string)) cusips
    List.iter (fun x -> fields.AppendValue(x:string)) mnemonics
    [request]

  let sendRequests (session:Session) (requestList:Request list) =
    let reqEventQueue = EventQueue()
    for req:Request in requestList do
      printfn $"Request ID :{req.RequestId}"
      printfn $"{req.ToString()}"
      printfn $"Use simplified auth session's authorized identity"
      let dIdentity = session.GetAuthorizedIdentity()
      session.SendRequest(req, dIdentity, reqEventQueue, new CorrelationID("Request " + req.RequestId)) |> ignore
    requestList, reqEventQueue, session

  let processReferenceResponseEvent (eventObj:Event) =
    seq { for msg:Message in eventObj do
            let response = { RequestId = None; ResponseError = None; SecurityData = None }
            let responseError = { ResponseError.Category = None; Message = None }
            let securityData = { Security = None; FieldData = None; FieldDataExceptions = None; SecurityError = None }
            let securityError = { SecurityError.Category = None; Message = None }

            // This is a dead end path. need to figure way to cause a response error before I can implement and test
            match msg.HasElement(RESPONSE_ERROR) with
            | true -> { response with 
                          ResponseError = Some ({ responseError with 
                                                    Category = Some(msg.GetElementAsString(CATEGORY)) 
                                                    Message = Some(msg.GetElementAsString(MESSAGE)) }) } |> ignore
            | _ -> response |> ignore
      
            let securities = msg.GetElement(SECURITY_DATA)

            let processSecurity (security:Element) (secData:SecurityData) =
              let ticker = security.GetElementAsString(SECURITY)
              let fields = security.GetElement(FIELD_DATA)

              let processBulkField (refBulkField:Element) =
                for bulkValue in refBulkField.Elements do
                  let bulkElement = bulkValue.GetValueAsElement()
                  // Read each field in Bulk data
                  for elem in bulkElement.Elements do
                    elem.GetElement(0) |> ignore
                { FieldDatum.Name = Some("Danger! Bulk Field Processing not enabled!"); Value = Some("Nothing here!") }

              let processRefField (refField:Element) =
                { Name = Some(refField.Name.ToString()); Value = Some(refField.GetValueAsString()) }

              let processFieldElements (fields:Element) (i:int) =
                let field = fields.GetElement i
                match field.IsArray with
                | true -> [processBulkField field]
                | false -> [processRefField field]

              let fieldExceptions = security.GetElement(FIELD_EXCEPTIONS)

              let processFieldExceptions (fieldExceptions:Element) i =
                  let fieldException = fieldExceptions.GetValueAsElement i
                  let errorInfo = { ErrorInfo.Category = Some(fieldException.GetElement(ERROR_INFO).GetElementAsString(CATEGORY));
                                    Message = Some(fieldException.GetElement(ERROR_INFO).GetElementAsString(MESSAGE)) }
                  [{ FieldId = Some(fieldException.GetElementAsString(FIELD_ID));
                    Message = None; 
                    ErrorInfo = Some(errorInfo) }]
        
              [{ secData with 
                  Security = Some(ticker)
                  FieldData = Some(List.collect (fun i -> processFieldElements fields i) [0..fields.NumElements - 1])
                  FieldDataExceptions = Some(List.collect (fun i -> processFieldExceptions fieldExceptions i) [0..fieldExceptions.NumValues - 1])
                  SecurityError = Some(match security.HasElement("securityError") with
                                      | true -> { securityError with
                                                    Category = Some ((security.GetElement(SECURITY_ERROR)).GetElementAsString(CATEGORY))
                                                    Message = Some ((security.GetElement(SECURITY_ERROR)).GetElementAsString(MESSAGE)) }
                                      | _ -> securityError)}]                                        
      
            { response with
                        RequestId = Some(msg.RequestId)
                        ResponseError = Some(responseError)
                        SecurityData = Some(List.collect (fun i -> processSecurity (securities.GetValueAsElement i) securityData) [0..securities.NumValues - 1]) }
      }

  let makeJson (response:seq<ReferenceDataResponse>) =
    Seq.toList (seq { for item in response do
                        Json.serialize item.RequestId
                        Json.serialize item.ResponseError
                        Json.serialize item.SecurityData })
      
  let handleEvents (requests:Request list, reqEventQueue:EventQueue, session:Session) =
    let rec handleSingleEvent (request:Request) = 
      try
        let eventObj = reqEventQueue.NextEvent()
        match eventObj.Type with
        | Event.EventType.REQUEST_STATUS -> processMiscEvents eventObj |> makeJson
        | Event.EventType.ADMIN -> processAdminEvent eventObj |> makeJson
        | Event.EventType.AUTHORIZATION_STATUS -> processAuthEvent eventObj session |> makeJson
        | Event.EventType.PARTIAL_RESPONSE -> processReferenceResponseEvent eventObj |> makeJson |> ignore
                                              handleSingleEvent request
        | Event.EventType.RESPONSE -> processReferenceResponseEvent eventObj |> makeJson
        | _ -> processMiscEvents eventObj |> makeJson
      with
        | ex -> failwithf "%s" ex.Message

    List.map (fun request -> handleSingleEvent request) requests

  // implement abstract member of Logging.Callback interface
  type LoggingCallback() =
    interface Logging.Callback with
      member this.OnMessage(threadId:int64, level:TraceLevel, dateTime:Datetime, loggerName:string,
       message:string) =
        printfn $"{dateTime} {loggerName} [{level}] Thread ID = {threadId} {message}"          
  
  let registerCallback level =
    Logging.RegisterCallback(LoggingCallback(), level)

  // [<EntryPoint>]
  // let main args =
    // BLPAPI logging
  registerCallback TraceLevel.Warning

  let options = setOptions serverHost serverPort appName

  let dSession = createSession options

  match dSession.Start() with
  | true -> ()
  | false -> printfn "Failed to start session"

  match dSession.OpenService requestService with
  | true -> () 
  | false -> Console.Error.WriteLine("Failed to open " + requestService)
              dSession.Stop()

  let result = createRequest dSession requestService |> 
                createSecuritiesRequestList |> 
                sendRequests dSession |>
                handleEvents
  
  List.iter (printfn "%A") result

  dSession.Stop()
  0