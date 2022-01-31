open Microsoft.Extensions.Hosting
// open Microsoft.Extensions.DependencyInjection
// open Microsoft.Azure.Functions.Worker
// open Microsoft.Azure.Functions.Worker.Extensions
// open Microsoft.Azure.Functions.Worker.Configuration

// let host =
//     HostBuilder()
//         .ConfigureFunctionsWorkerDefaults()
//         // .ConfigureServices(fun s ->
//         //     s.AddSingleton<IHttpResponderService, DefaultHttpResponderService>()
//         // )
//         .Build()

// task {
//     do! host.RunAsync()
// } |> ignore

HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .Build()
    .Run()
