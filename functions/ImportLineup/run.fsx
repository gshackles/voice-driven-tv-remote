#load "Database.fs"
open System.Collections.Generic
open FSharp.Data

type Listings = XmlProvider<"D:\\home\\site\\wwwroot\\ImportLineup\\schedule-sample.xml">

let addShow (log: TraceWriter) (channelLookup: IDictionary<string, int>) (show: Listings.Programme) =
    match channelLookup.TryGetValue show.Channel with
    | (true, channelId) ->
        let title = match (show.Title.Value, show.SubTitle) with
                    | "Movie", Some(subtitle) -> subtitle.Value.Trim()
                    | title, _ -> title.Trim()
        let description = if show.Desc.IsSome then show.Desc.Value.Value else ""
        
        Database.addShow title show.Start show.Stop channelId description show.Category.Value

        sprintf "Added show : %s" title |> log.Info
    | (false, _) -> ()

let Run(xml: string, name: string, log: TraceWriter, outputQueueItem: byref<string>) =
    sprintf "Starting import for: %s" name |> log.Info

    let channelLookup = Database.getChannelLookup()
    let listings = Listings.Parse xml

    Database.clearShows()

    listings.Programmes
    |> Array.take 10000
    |> Array.iter (addShow log channelLookup)

    outputQueueItem <- name

    sprintf "Finished import for: %s" name |> log.Info