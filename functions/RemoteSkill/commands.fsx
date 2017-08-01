module Commands

open System
open FSharp.Data

type CommandsResponse = JsonProvider<""" {"commands":[{"name":"VolumeDown","slug":"volume-down","label":"Volume Down"}]} """>
type StatusResponse = JsonProvider<""" {"off":false,"current_activity":{"id":"22754325","slug":"watch-tv","label":"Watch TV","isAVActivity":true}} """>

let private makeRequest method urlPath =
    let url = sprintf "%s/%s" (Environment.GetEnvironmentVariable("HarmonyApiUrlBase")) urlPath
    let authHeader = "Authorization", (Environment.GetEnvironmentVariable("HarmonyApiKey"))

    Http.RequestString(url, httpMethod = method, headers = [authHeader])

let getCommand (label: string) =
    makeRequest "GET" "commands"
    |> CommandsResponse.Parse
    |> fun res -> res.Commands
    |> Seq.tryFind (fun command -> command.Label.ToLowerInvariant() = label.ToLowerInvariant())

let executeCommand commandSlug = sprintf "commands/%s" commandSlug |> makeRequest "POST" |> ignore

let watchTV() = 
    makeRequest "GET" "status" 
    |> StatusResponse.Parse 
    |> fun res -> match res.CurrentActivity.Slug with
                  | "watch-tv" -> ()
                  | _ ->
                    makeRequest "POST" "activities/watch-tv" |> ignore
                    Async.Sleep(15*1000) |> Async.RunSynchronously

let changeChannel number = 
    string number |> Seq.map string |> Seq.iter executeCommand
    executeCommand "select"