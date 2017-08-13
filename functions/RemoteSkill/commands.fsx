module Commands

open System
open System.Text
open Microsoft.Azure.Devices
open FSharp.Data
open FSharp.Data.SqlClient

[<Literal>]
let configFile = "D:\\home\\site\\wwwroot\\RemoteSkill\\app.config"

let getCommand (label: string) =
    use operation = Telemetry.startOperation "GetCommand"
    use cmd = new SqlCommandProvider<"SELECT Slug FROM AvailableCommand WHERE Label=@label", "name=TVListings", ConfigFile=configFile, SingleRow=true>()
    cmd.Execute(label = label)

let serviceClient = ServiceClient.CreateFromConnectionString (Environment.GetEnvironmentVariable("IoTHubConnectionString"))

let executeCommand commandSlug = 
    use operation = Telemetry.startOperation "ExecuteCommand"
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
    use operation = Telemetry.startOperation "ChangeChannel"
    string number |> Seq.map string |> Seq.iter executeCommand
    executeCommand "select"