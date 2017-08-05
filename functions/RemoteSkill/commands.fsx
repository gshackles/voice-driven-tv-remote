module Commands

open System
open System.Text
open FSharp.Data
open Microsoft.Azure.Devices

type CommandsResponse = JsonProvider<""" {"commands":[{"name":"VolumeDown","slug":"volume-down","label":"Volume Down"}]} """>
type StatusResponse = JsonProvider<""" {"off":false,"current_activity":{"id":"22754325","slug":"watch-tv","label":"Watch TV","isAVActivity":true}} """>

let serviceClient = ServiceClient.CreateFromConnectionString (Environment.GetEnvironmentVariable("IoTHubConnectionString"))

let private makeRequest method urlPath =
    let url = sprintf "%s/%s" (Environment.GetEnvironmentVariable("HarmonyApiUrlBase")) urlPath
    let authHeader = "Authorization", (Environment.GetEnvironmentVariable("HarmonyApiKey"))

    Http.RequestString(url, httpMethod = method, headers = [authHeader])

let getCommand (label: string) =
    makeRequest "GET" "commands"
    |> CommandsResponse.Parse
    |> fun res -> res.Commands
    |> Seq.tryFind (fun command -> command.Label.ToLowerInvariant() = label.ToLowerInvariant())

let executeCommand commandSlug = 
    async {
        sprintf "harmony-api/hubs/living-room/command;%s" commandSlug
        |> Encoding.ASCII.GetBytes
        |> fun bytes -> new Message(bytes)
        |> fun message -> serviceClient.SendAsync("harmony-bridge", message)
        |> Async.AwaitIAsyncResult 
        |> Async.Ignore
        |> ignore

        do! Async.Sleep 250
    } |> Async.RunSynchronously
    
let changeChannel number = 
    string number |> Seq.map string |> Seq.iter executeCommand
    executeCommand "select"